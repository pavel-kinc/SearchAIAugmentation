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

        public async Task DownloadConfiguration(EnhancerConfiguration configuration, string filePath = "config.json", bool hideSecrets = true)
        {
            var json = GetConfigurationJson(configuration, hideSecrets);
            await File.WriteAllTextAsync(filePath, json);
        }

        public byte[] ExportConfigurationToBytes(EnhancerConfiguration configuration, bool hideSecrets = true)
        {
            var json = GetConfigurationJson(configuration, hideSecrets);
            return Encoding.UTF8.GetBytes(json);
        }

        public async Task<EnhancerConfiguration?> ImportConfigurationFromFile(string filePath)
        {
            var json = await File.ReadAllTextAsync(filePath);
            return JsonConvert.DeserializeObject<EnhancerConfiguration>(json);
        }

        public EnhancerConfiguration? ImportConfigurationFromBytes(byte[] jsonBytes)
        {
            var json = Encoding.UTF8.GetString(jsonBytes);
            return JsonConvert.DeserializeObject<EnhancerConfiguration>(json);
        }

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

        public async Task<ErrorOr<IList<ResultModel>>> ProcessPipelineAsync(PipelineModel pipeline, IEnumerable<PipelineRun> entries, CancellationToken cancellationToken = default)
        {
            try
            {
                if (!entries.Any())
                {
                    return Error.Unexpected("No input specified, nothing to proccess");
                }
                var cb = new ConcurrentBag<ResultModel>();
                await Parallel.ForEachAsync(entries, async (context, _) =>
                {
                    var pipelineRes = await _pipelineOrchestrator.RunPipelineAsync(pipeline, context, cancellationToken);
                    var resultModel = new ResultModel
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
                return Error.Failure($"{nameof(ProcessPipelineAsync)} failed", ex.Message);
            }
        }

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

        public async Task<ErrorOr<IList<ResultModel>>> ProcessConfiguration(EnhancerConfiguration config, IEnumerable<Entry> entries, Kernel? kernel = null, CancellationToken cancellationToken = default)
        {
            var settings = CreatePipelineSettingsFromConfig(config.PromptConfiguration, config.PipelineAdditionalSettings, config.KernelConfiguration, kernel);

            if (settings.IsError)
            {
                return settings.Errors;
            }

            var pipeline = new PipelineModel(settings.Value, config.Steps);

            return await ProcessPipelineAsync(pipeline, entries.Select(x => new PipelineRun(x)), cancellationToken);

            //TODO should this be here? (maybe like sk.invoke and hope there are some plugins? - since there are 3 ways to kernel here, also i would need some plugins in my creation, but i could resolve plugins by injection (common interface))
            //TODO it could also require check for openai, options and some uniform way to work with results, or just put it outside of this method and just work with it there, but it requires same arguments prolly
            //TODO ye just put it outside of here, this is bad, just if else with this config i guess
            //if (config.UseAutomaticFunctionCalling)
            //{
            //    return await HandleAutomaticFunctionCalling(sk, entries.FirstOrDefault());
            //}

            //if(config.PipeLineSteps?.Any() != true)
            //{
            //    return Error.Unexpected("No pipeline steps defined in configuration.");
            //}

            //delete
            //_pipelineContextService.SetCurrentContext(context);



            //change to list of results, deal with errors in result creation

            // will be needed to search by params/config
            // refactor into pipeline
            //var sk = _semanticKernelManager.CreateKernel(skData!);
            //_semanticKernelManager.AddPluginToSemanticKernel<SemanticSlicerChunkGenerator>(sk!);
            //sk!.Plugins.TryGetPlugin(typeof(SemanticSlicerChunkGenerator).Name, out var plugin);
            //var service = sk.GetRequiredService<IChunkGenerator>();



            //var prompt = "{{SemanticSlicerChunkGenerator.generate_chunks_from_string $rawText}}";
            //var arg = "Multiple Implementations: If there are multiple implementations of generate_chunks_from_string, ensure that the correct one is invoked by specifying the appropriate function name or handling the selection logic.\n\nError Handling: Implement robust error handling to manage scenarios where the function might not be available or the invocation fails.\n\nFunction Signatures: Ensure that the function signatures match the expected parameters to avoid runtime errors.";
            //var c = new PromptTemplateConfig
            //{
            //    Template = prompt,
            //    TemplateFormat = PromptTemplateConfig.SemanticKernelTemplateFormat,
            //    Name = "ChunkGenTemplate"
            //};
            //var factory = new KernelPromptTemplateFactory();
            //var b = factory.TryCreate(c, out var promptTemplate);
            //var rendered = await promptTemplate.RenderAsync(sk, new KernelArguments { { "rawText", arg } });
            //var res = await sk.InvokePromptAsync(prompt, new KernelArguments { { "rawText", arg } });
            //var result = await sk.InvokeAsync(typeof(SemanticSlicerChunkGenerator).Name, "generate_chunks_from_string", new KernelArguments{ { "rawText", arg } });
            //var result2 = await sk.InvokePromptAsync($"Generate chunks from this string and return only list of strings: {arg}", new(settings));
            //var val1 = result.GetValue<IList<string>>();
            //var val = result2.GetValue<string>();
            //var val = res.GetValue<string>();
            //var textSearch = _searchProviderManager.CreateTextSearch(searchData!)!;

            //var strings = new List<string> { "Pes šel do lesa.", "Kočka spí v koši.", "Auto jede rychle." };
            //var embeddingGenerator = sk.GetRequiredService<IEmbeddingGenerator<string, Embedding<float>>>();
            //var emb = await embeddingGenerator.GenerateAsync("závodní vůz.");
            //var vectorStore = new InMemoryVectorStore();
            //var collection = new InMemoryCollection<string, VectorStore>("myCollection");
            //await collection.EnsureCollectionExistsAsync();
            ////var embeddings = await embeddingGenerator.GenerateAsync(strings.ToArray());

            //for (int i = 0; i < strings.Count; i++)
            //{
            //    var e = await embeddingGenerator.GenerateAsync(strings[i]);
            //    await collection.UpsertAsync(new VectorStore
            //    {
            //        Id = $"doc-{i}",
            //        Text = strings[i],
            //        Embedding = e.Vector
            //    });
            //}
            //var searchResult = collection.SearchAsync(emb, top: 3);

            //await foreach (var record in searchResult)
            //{
            //    Console.WriteLine($"Best match: {record.Record.Text}");
            //    Console.WriteLine($"Score: {record.Score}");
            //}

            await Parallel.ForEachAsync(entries, async (entry, _) =>
            {
                //var query = entry.QueryString!;
                //var res = await _searchProviderManager.GetSearchResults(textSearch, query);
                //var searchResults = await res.Results.ToListAsync();
                //var usedUrls = searchResults.Where(x => !string.IsNullOrEmpty(x.Link)).Select(x => x.Link!);
                //temporary
                //if (useScraper)
                //{
                //    //will be needed some specifications from config what to search for maybe?
                //    var rawScrapedContent = await _searchWebScraper.ScrapeDataFromUrlsAsync(usedUrls);
                //    var chunks = _chunkGenerator.GenerateChunksFromData(rawScrapedContent);
                //    resultView.SearchResult = _chunkRanker.ExtractRelevantDataFromChunks(chunks, query);
                //}
                //else
                //{
                //    //this uses snippets from search only
                //    resultView.SearchResult = string.Join('\n', searchResults.Select(x => x.Value));
                //}
                //resultView.Prompt = PromptUtility.BuildPrompt(promptConf, query, resultView.SearchResult);
                ////resultView.AIResult = await _semanticKernelManager.GetAICompletionResult(sk!, resultView.Prompt);
                //resultView.AIResult.UsedURLs = usedUrls;
            });
        }

        // only for not search and KB normal functionality (when you want to give data without defining knowledge base), for greater control setup normal knowledge base
        public IKnowledgeBaseContainer CreateDefaultDataContainer<TModel>(IEnumerable<TModel> data)
            where TModel : class
        {
            Type recordType = typeof(KnowledgeRecord<>).MakeGenericType(typeof(TModel));
            Type kbType = typeof(KnowledgeBaseDefault<,>).MakeGenericType(recordType, typeof(TModel));
            var kb = (KnowledgeBaseDefault<KnowledgeRecord<TModel>, TModel>)ActivatorUtilities.CreateInstance(_serviceProvider, kbType);
            return new KnowledgeBaseDataContainer<KnowledgeRecord<TModel>, TModel>(kb, data);
        }

        // only for not search and KB normal functionality (when you want to give data without defining knowledge base) with specified record, for greater control setup normal knowledge base
        public IKnowledgeBaseContainer CreateDefaultDataContainer<TRecord, TModel>(IEnumerable<TModel> data)
            where TModel : class
            where TRecord : KnowledgeRecord<TModel>, new()
        {
            Type recordType = typeof(TRecord);
            Type kbType = typeof(KnowledgeBaseDefault<,>).MakeGenericType(recordType, typeof(TModel));
            var kb = (KnowledgeBaseDefault<TRecord, TModel>)ActivatorUtilities.CreateInstance(_serviceProvider, kbType);
            return new KnowledgeBaseDataContainer<TRecord, TModel>(kb, data);
        }

        public ErrorOr<PipelineModel> CreateDefaultSearchPipeline(IEnumerable<IKnowledgeBaseContainer> containers, PromptConfiguration? promptConf = null, PipelineAdditionalSettings? pipelineSettings = null, KernelConfiguration? kernelData = null, Kernel? kernel = null)
        {
            return CreateDefaultSearchPipelineCommon(containers, promptConf, pipelineSettings, kernelData, kernel, true);
        }

        public ErrorOr<PipelineModel> CreateDefaultSearchPipelineWithoutGenerationStep(IEnumerable<IKnowledgeBaseContainer> containers, PromptConfiguration? promptConf = null, PipelineAdditionalSettings? pipelineSettings = null, KernelConfiguration? kernelData = null, Kernel? kernel = null)
        {
            return CreateDefaultSearchPipelineCommon(containers, promptConf, pipelineSettings, kernelData, kernel, false);
        }

        public IEnumerable<IPipelineStep> CreateDefaultGoogleSearchPipelineSteps(string googleApiKey, string googleEngine, GoogleSearchFilterModel? searchFilter = null, GoogleSettings? googleSettings = null, UrlRecordFilter? filter = null)
        {
            searchFilter ??= new GoogleSearchFilterModel();
            googleSettings ??= new GoogleSettings() { SearchApiKey = googleApiKey, Engine = googleEngine };
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
