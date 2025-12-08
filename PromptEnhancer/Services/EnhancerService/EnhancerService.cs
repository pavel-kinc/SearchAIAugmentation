using ErrorOr;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Newtonsoft.Json;
using PromptEnhancer.AIUtility.ChatHistory;
using PromptEnhancer.CustomJsonResolver;
using PromptEnhancer.KnowledgeBaseCore;
using PromptEnhancer.KnowledgeBaseCore.Examples;
using PromptEnhancer.KnowledgeBaseCore.Interfaces;
using PromptEnhancer.KnowledgeRecord;
using PromptEnhancer.KnowledgeSearchRequest.Examples;
using PromptEnhancer.Models;
using PromptEnhancer.Models.Configurations;
using PromptEnhancer.Models.Enums;
using PromptEnhancer.Models.Examples;
using PromptEnhancer.Models.Pipeline;
using PromptEnhancer.Pipeline.Interfaces;
using PromptEnhancer.Pipeline.PromptEnhancerSteps;
using PromptEnhancer.SK.Interfaces;
using System.Collections.Concurrent;
using System.Text;

namespace PromptEnhancer.Services.EnhancerService
{
    /// <summary>
    /// Provides services for creating, managing, and processing configurations, pipelines, and data containers for
    /// enhancing workflows using semantic kernels, AI models, and knowledge bases.
    /// </summary>
    /// <remarks>The <see cref="EnhancerService"/> class offers a variety of methods to configure and execute
    /// pipelines, import and export configurations, and manage knowledge bases. It integrates with semantic kernel
    /// managers, pipeline orchestrators, and external services such as Google Knowledge Base.</remarks>
    public class EnhancerService : IEnhancerService
    {
        private readonly ISemanticKernelManager _semanticKernelManager;
        private readonly IPipelineOrchestrator _pipelineOrchestrator;
        private readonly Kernel? _kernel;
        private readonly IServiceProvider _serviceProvider;
        private readonly GoogleKnowledgeBase _googleKB;
        private readonly ILogger<EnhancerService> _logger;

        public EnhancerService(ISemanticKernelManager semanticKernelManager, IPipelineOrchestrator pipelineOrchestrator, IServiceProvider serviceProvider, GoogleKnowledgeBase googleKB, ILogger<EnhancerService> logger, Kernel? kernel = null)
        {
            _semanticKernelManager = semanticKernelManager;
            _pipelineOrchestrator = pipelineOrchestrator;
            _kernel = kernel;
            _serviceProvider = serviceProvider;
            _googleKB = googleKB;
            _logger = logger;
        }

        /// <inheritdoc/>
        // supports single completion and embedding
        public EnhancerConfiguration CreateDefaultConfiguration(string? aiApiKey = null, AIProviderEnum aiProvider = AIProviderEnum.OpenAI, string aiModel = "gpt-4o-mini", string? embeddingModel = "text-embedding-3-small")
        {
            var enhancerConfiguration = new EnhancerConfiguration();
            enhancerConfiguration.KernelConfiguration = new KernelConfiguration
            {
                AIApiKey = aiApiKey,
                Model = aiModel,
                Provider = aiProvider,
                EmbeddingModel = embeddingModel,
                UseLLMConfigForEmbeddings = embeddingModel is not null && aiProvider == AIProviderEnum.OpenAI,
            };
            return enhancerConfiguration;
        }

        /// <inheritdoc/>
        public async Task DownloadConfiguration(EnhancerConfiguration configuration, string filePath = "config.json", bool hideSecrets = true)
        {
            var json = GetConfigurationJson(configuration, hideSecrets);
            await File.WriteAllTextAsync(filePath, json);
        }

        /// <inheritdoc/>
        public byte[] ExportConfigurationToBytes(EnhancerConfiguration configuration, bool hideSecrets = true)
        {
            var json = GetConfigurationJson(configuration, hideSecrets);
            return Encoding.UTF8.GetBytes(json);
        }

        /// <inheritdoc/>
        public async Task<EnhancerConfiguration?> ImportConfigurationFromFile(string filePath)
        {
            var json = await File.ReadAllTextAsync(filePath);
            return JsonConvert.DeserializeObject<EnhancerConfiguration>(json);
        }

        /// <inheritdoc/>
        public EnhancerConfiguration? ImportConfigurationFromBytes(byte[] jsonBytes)
        {
            var json = Encoding.UTF8.GetString(jsonBytes);
            return JsonConvert.DeserializeObject<EnhancerConfiguration>(json);
        }

        /// <inheritdoc/>
        public IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponse(PipelineSettings settings, PipelineRun context, CancellationToken ct = default)
        {
            var chatClient = settings.Kernel.GetRequiredService<IChatClient>(settings.Settings.ChatClientKey);
            List<ChatMessage> history = ChatHistoryUtility.AddToChatHistoryPipeline(context);
            context.ChatHistory = history;
            if (ChatHistoryUtility.GetHistoryLength(history) > settings.Settings.MaximumInputLength)
            {
                throw new ArgumentOutOfRangeException(nameof(context), ChatHistoryUtility.GetInputSizeExceededLimitMessage(nameof(GetStreamingResponse)));
            }

            return chatClient.GetStreamingResponseAsync(history, settings.Settings.ChatOptions, ct);
        }

        /// <inheritdoc/>
        public async Task<ErrorOr<IList<PipelineResultModel>>> ExecutePipelineAsync(PipelineModel pipeline, IEnumerable<PipelineRun> entries, CancellationToken cancellationToken = default)
        {
            try
            {
                if (!entries.Any())
                {
                    return Error.Unexpected("No input specified, nothing to proccess");
                }
                var cb = new ConcurrentBag<PipelineResultModel>();
                await Parallel.ForEachAsync(entries, async (context, _) =>
                {
                    var pipelineRes = await _pipelineOrchestrator.RunPipelineAsync(pipeline, context, cancellationToken);
                    var resultModel = new PipelineResultModel
                    {
                        Result = context,
                        Errors = pipelineRes.ErrorsOrEmptyList,
                    };
                    cb.Add(resultModel);
                });
                return cb.ToList();
            }
            catch (Exception ex)
            {
                return Error.Failure($"{nameof(ExecutePipelineAsync)} failed", ex.Message);
            }
        }

        /// <inheritdoc/>
        public ErrorOr<PipelineSettings> CreatePipelineSettingsFromConfig(PromptConfiguration promptConf, PipelineAdditionalSettings pipelineSettings, KernelConfiguration? kernelData = null, Kernel? kernel = null)
        {
            var sk = kernel ?? _kernel;
            if (sk is null && kernelData is not null)
            {
                var configs = _semanticKernelManager.ConvertConfig(kernelData);
                if (configs.IsError)
                {
                    return configs.Errors;
                }
                var createdKernel = _semanticKernelManager.CreateKernel(configs.Value);
                if (createdKernel.IsError)
                {
                    return createdKernel.Errors;
                }
                sk = createdKernel.Value;
            }

            if (sk is null)
            {
                return Error.Failure("No kernel could be created or resolved.");
            }
            return new PipelineSettings(sk, _serviceProvider, pipelineSettings, promptConf);
        }

        /// <inheritdoc/>
        public async Task<ErrorOr<IList<PipelineResultModel>>> ProcessConfiguration(EnhancerConfiguration config, IEnumerable<Entry> entries, Kernel? kernel = null, CancellationToken cancellationToken = default)
        {
            var settings = CreatePipelineSettingsFromConfig(config.PromptConfiguration, config.PipelineAdditionalSettings, config.KernelConfiguration, kernel);

            if (settings.IsError)
            {
                return settings.Errors;
            }

            var pipeline = new PipelineModel(settings.Value, config.Steps);

            return await ExecutePipelineAsync(pipeline, entries.Select(x => new PipelineRun(x)), cancellationToken);
        }

        /// <inheritdoc/>
        // only for not search and KB normal functionality (when you want to give data without defining knowledge base), for greater control setup normal knowledge base
        public IKnowledgeBaseContainer CreateDefaultDataContainer<TModel>(IEnumerable<TModel> data)
            where TModel : class
        {
            Type recordType = typeof(KnowledgeRecord<>).MakeGenericType(typeof(TModel));
            Type kbType = typeof(KnowledgeBaseDataDefault<,>).MakeGenericType(recordType, typeof(TModel));
            var kb = (KnowledgeBaseDataDefault<KnowledgeRecord<TModel>, TModel>)ActivatorUtilities.CreateInstance(_serviceProvider, kbType);
            return new KnowledgeBaseDataContainer<KnowledgeRecord<TModel>, TModel>(kb, data);
        }

        /// <inheritdoc/>
        // only for not search and KB normal functionality (when you want to give data without defining knowledge base) with specified record, for greater control setup normal knowledge base
        public IKnowledgeBaseContainer CreateDefaultDataContainer<TRecord, TModel>(IEnumerable<TModel> data)
            where TModel : class
            where TRecord : KnowledgeRecord<TModel>, new()
        {
            Type recordType = typeof(TRecord);
            Type kbType = typeof(KnowledgeBaseDataDefault<,>).MakeGenericType(recordType, typeof(TModel));
            var kb = (KnowledgeBaseDataDefault<TRecord, TModel>)ActivatorUtilities.CreateInstance(_serviceProvider, kbType);
            return new KnowledgeBaseDataContainer<TRecord, TModel>(kb, data);
        }

        /// <inheritdoc/>
        public ErrorOr<PipelineModel> CreateDefaultSearchPipeline(IEnumerable<IKnowledgeBaseContainer> containers, PromptConfiguration? promptConf = null, PipelineAdditionalSettings? pipelineSettings = null, KernelConfiguration? kernelData = null, Kernel? kernel = null)
        {
            return CreateDefaultSearchPipelineCommon(containers, promptConf, pipelineSettings, kernelData, kernel, true);
        }

        /// <inheritdoc/>
        public ErrorOr<PipelineModel> CreateDefaultSearchPipelineWithoutGenerationStep(IEnumerable<IKnowledgeBaseContainer> containers, PromptConfiguration? promptConf = null, PipelineAdditionalSettings? pipelineSettings = null, KernelConfiguration? kernelData = null, Kernel? kernel = null)
        {
            return CreateDefaultSearchPipelineCommon(containers, promptConf, pipelineSettings, kernelData, kernel, false);
        }

        /// <inheritdoc/>
        public IEnumerable<IPipelineStep> CreateDefaultGoogleSearchPipelineSteps(string googleApiKey, string googleEngine, GoogleSearchFilterModel? searchFilter = null, GoogleSettings? googleSettings = null, UrlRecordFilter? filter = null, bool useScraper = false)
        {
            searchFilter ??= new GoogleSearchFilterModel();
            googleSettings ??= new GoogleSettings() { SearchApiKey = googleApiKey, Engine = googleEngine, UseScraper = useScraper };
            var request = new GoogleSearchRequest
            {
                Settings = googleSettings,
                Filter = searchFilter
            };

            var container = new KnowledgeBaseContainer<KnowledgeUrlRecord, GoogleSearchFilterModel, GoogleSettings, UrlRecordFilter, UrlRecord>(_googleKB, request, filter);
            return
                [
                    new PreprocessStep(),
                    new KernelContextPluginsStep(),
                    //new QueryParserStep(maxSplit: 2),
                    new MultipleSearchStep([container], allowAutoChoice: false, isRequired: true),
                    new ProcessEmbeddingStep(skipGenerationForEmbData: true, isRequired: true),
                    new ProcessRankStep(isRequired: true),
                    new ProcessFilterStep(new RecordPickerOptions(){MinScoreSimilarity = 0.3d, Take = 4, OrderByScoreDescending = true}, isRequired: true),
                    new PostProcessCheckStep(),
                    new PromptBuilderStep(isRequired: true),
                    new GenerationStep(isRequired: true),
                ];
        }

        /// <summary>
        /// Creates a default search pipeline model with the specified configuration and settings.
        /// </summary>
        /// <remarks>This method constructs a pipeline model with a predefined sequence of steps,
        /// including preprocessing, multiple search,  embedding processing, ranking, filtering, and prompt building. If
        /// <paramref name="addGenerationStep"/> is set to  <see langword="true"/>, a generation step is appended to the
        /// pipeline.</remarks>
        /// <param name="containers">A collection of knowledge base containers used for the search pipeline.</param>
        /// <param name="promptConf">The prompt configuration to use for the pipeline. If null, a default configuration is applied.</param>
        /// <param name="pipelineSettings">Additional settings for the pipeline. If null, default settings are applied.</param>
        /// <param name="kernelData">The kernel configuration to use for the pipeline. Can be null if not required.</param>
        /// <param name="kernel">The kernel instance to use for the pipeline. Can be null if not required.</param>
        /// <param name="addGenerationStep">A boolean value indicating whether to include a generation step in the pipeline.  <see langword="true"/> to
        /// include the generation step; otherwise, <see langword="false"/>.</param>
        /// <returns>An <see cref="ErrorOr{T}"/> containing the created <see cref="PipelineModel"/> if successful, or an error if
        /// the pipeline configuration is invalid.</returns>
        private ErrorOr<PipelineModel> CreateDefaultSearchPipelineCommon(IEnumerable<IKnowledgeBaseContainer> containers, PromptConfiguration? promptConf, PipelineAdditionalSettings? pipelineSettings, KernelConfiguration? kernelData, Kernel? kernel, bool addGenerationStep)
        {
            var defaultConfig = new EnhancerConfiguration();
            var settings = CreatePipelineSettingsFromConfig(promptConf ?? defaultConfig.PromptConfiguration, pipelineSettings ?? defaultConfig.PipelineAdditionalSettings, kernelData, kernel);
            if (settings.IsError)
            {
                return settings.Errors;
            }
            var steps = new List<IPipelineStep>
            {
                new PreprocessStep(),
                new MultipleSearchStep(containers, allowAutoChoice: true, isRequired: true),
                new ProcessEmbeddingStep(skipGenerationForEmbData: true, isRequired: true),
                new ProcessRankStep(isRequired: true),
                new ProcessFilterStep(new RecordPickerOptions(){MinScoreSimilarity = 0.2d, Take = 5, OrderByScoreDescending = true}, isRequired: true),
                new PostProcessCheckStep(),
                new PromptBuilderStep(isRequired: true),
            };

            if (addGenerationStep)
            {
                steps.Add(new GenerationStep(isRequired: true));
            }

            return new PipelineModel(settings.Value, steps);
        }

        /// <summary>
        /// Converts the specified <see cref="EnhancerConfiguration"/> object to a JSON string.
        /// </summary>
        /// <remarks>The output JSON is formatted with indented styling for readability. If <paramref
        /// name="hideSecrets"/>  is set to <see langword="true"/>, a custom contract resolver is used to mask sensitive
        /// fields.</remarks>
        /// <param name="configuration">The configuration object to serialize.</param>
        /// <param name="hideSecrets">A value indicating whether sensitive information in the configuration should be hidden. If <see
        /// langword="true"/>, sensitive fields are masked in the output; otherwise, all fields are included.</param>
        /// <returns>A JSON-formatted string representation of the <paramref name="configuration"/> object.</returns>
        private string GetConfigurationJson(EnhancerConfiguration configuration, bool hideSecrets = true)
        {
            var settings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented
            };

            if (hideSecrets)
            {
                settings.ContractResolver = new SensitiveContractResolver();
            }

            return JsonConvert.SerializeObject(configuration, settings);
        }
    }
}
