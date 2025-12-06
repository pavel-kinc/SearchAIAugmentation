using ErrorOr;
using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel;
using PromptEnhancer.KnowledgeBaseCore;
using PromptEnhancer.KnowledgeBaseCore.Examples;
using PromptEnhancer.KnowledgeBaseCore.Interfaces;
using PromptEnhancer.KnowledgeRecord;
using PromptEnhancer.KnowledgeRecord.Interfaces;
using PromptEnhancer.KnowledgeSearchRequest;
using PromptEnhancer.KnowledgeSearchRequest.Examples;
using PromptEnhancer.Models;
using PromptEnhancer.Models.Configurations;
using PromptEnhancer.Models.Enums;
using PromptEnhancer.Models.Examples;
using PromptEnhancer.Models.Pipeline;
using PromptEnhancer.Pipeline;
using PromptEnhancer.Pipeline.PromptEnhancerSteps;
using PromptEnhancer.Services.EnhancerService;
using PromptEnhancer.Services.PromptBuildingService;
using PromptEnhancer.SK.Interfaces;
using TaskChatDemo.KnowledgeBases;
using TaskChatDemo.Models;
using TaskChatDemo.Models.SearchFilterModels;
using TaskChatDemo.Models.Settings;
using TaskChatDemo.Models.TaskItem;

namespace TaskChatDemo.Services.EnhancerUtility
{
    public class EnhancerUtilityService : IEnhancerUtilityService
    {
        public const string GeminiServiceId = "gemini";
        public const string OpenAiServiceId = "openai";
        private readonly IPromptBuildingService _promptBuildingService;
        private readonly IEnhancerService _enhancerService;
        private readonly IConfiguration _configuration;
        private readonly ISemanticKernelManager _semanticKernelManager;
        private readonly TaskDataKnowledgeBase _taskDataKnowledgeBase;
        private readonly WorkItemKnowledgeBase _workItemKnowledgeBase;
        private readonly GoogleKnowledgeBase _googleKB;

        public EnhancerUtilityService(IEnhancerService enhancerService, IConfiguration configuration, ISemanticKernelManager skManager, TaskDataKnowledgeBase taskDataKnowledgeBase, WorkItemKnowledgeBase workItemKnowledgeBase, GoogleKnowledgeBase googleKB, IPromptBuildingService promptBuildingService)
        {
            _enhancerService = enhancerService;
            _configuration = configuration;
            _semanticKernelManager = skManager;
            _taskDataKnowledgeBase = taskDataKnowledgeBase;
            _workItemKnowledgeBase = workItemKnowledgeBase;
            _googleKB = googleKB;
            _promptBuildingService = promptBuildingService;
        }

        public ErrorOr<PipelineSettings> GetPipelineSettings()
        {
            var kernel = CreateKernel(_configuration["AIServices:OpenAI:ApiKey"]!, _configuration["AIServices:Gemini:ApiKey"]!);
            var enhancerConfig = _enhancerService.CreateDefaultConfiguration();

            enhancerConfig.PipelineAdditionalSettings = new PipelineAdditionalSettings()
            {
                //here to change client and generator for pipeline
                ChatClientKey = GeminiServiceId,
                GeneratorKey = OpenAiServiceId,
                KernelRequestSettings = new PromptExecutionSettings()
                {
                    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),
                    ServiceId = OpenAiServiceId
                },
            };

            enhancerConfig.PromptConfiguration.AdditionalInstructions =
                """
                You are an AI assistant helping a software development team manage their tasks and work items effectively.
                Provide clear, concise, and relevant information based on the context provided from their task and work item data.
                Do not use any formatting or newlines in your response. Use whole sentences.
                """;

            var settingsResult = _enhancerService.CreatePipelineSettingsFromConfig(enhancerConfig.PromptConfiguration, enhancerConfig.PipelineAdditionalSettings, enhancerConfig.KernelConfiguration, kernel);
            return settingsResult;
        }

        public async Task<PipelineRun> GetContextFromPipeline(string q, bool skipPipeline, Entry entry, PipelineSettings settings)
        {
            PipelineRun context;
            if (skipPipeline)
            {
                context = new PipelineRun(entry)
                {
                    UserPromptToLLM = q,
                    SystemPromptToLLM = _promptBuildingService.BuildSystemPrompt(settings.PromptConfiguration),
                };
            }
            else
            {
                var pipeline = GetPipeline(settings, settings.Kernel);
                var pipelineRes = await _enhancerService.ProcessPipelineAsync(pipeline, [new PipelineRun(entry)]);
                if (pipelineRes.IsError || pipelineRes.Value.FirstOrDefault()?.Result is null)
                {
                    throw new InvalidOperationException($"Pipeline failed: {pipelineRes.ErrorsOrEmptyList.Select(x => x.ToString())}");
                }
                var firstResponse = pipelineRes.Value.FirstOrDefault();
                if (firstResponse is null || !firstResponse.PipelineSuccess)
                {
                    throw new InvalidOperationException($"Pipeline failed for query: {firstResponse?.Errors.Select(x => x.Code)}");
                }
                context = pipelineRes.Value.FirstOrDefault()!.Result!;
            }

            return context;
        }
        private PipelineModel GetPipeline(PipelineSettings settings, Kernel kernel)
        {
            var taskItemRequest = new KnowledgeSearchRequest<SearchItemFilterModel, ItemDataSearchSettings>()
            {
                Filter = new SearchItemFilterModel(),
                Settings = new ItemDataSearchSettings() { Generator = kernel.GetRequiredService<IEmbeddingGenerator<string, Embedding<float>>>(OpenAiServiceId) }
            };
            var workItemRequest = new KnowledgeSearchRequest<SearchWorkItemFilterModel, WorkItemSearchSettings>()
            {
                Filter = new SearchWorkItemFilterModel(),
                Settings = new WorkItemSearchSettings(),
            };

            var containers = new List<IKnowledgeBaseContainer>()
            {
            new KnowledgeBaseContainer<KnowledgeRecord<TaskItemData>, SearchItemFilterModel, ItemDataSearchSettings, EmptyModelFilter<TaskItemData>, TaskItemData>(_taskDataKnowledgeBase, taskItemRequest, null),
            new KnowledgeBaseContainer<KnowledgeRecord<WorkItem>, SearchWorkItemFilterModel, WorkItemSearchSettings, EmptyModelFilter<WorkItem>, WorkItem>(_workItemKnowledgeBase, workItemRequest, null),
            };
            AddGoogleKnowledgeBaseIfDefined(containers);

            var steps = new List<PipelineStep>()
            {
                new PreprocessStep(),
                new KernelContextPluginsStep(),
                // new QueryParserStep(maxSplit: 2),
                new MultipleSearchStep(containers, isRequired: true),
                new ProcessEmbeddingStep(skipGenerationForEmbData: true, isRequired: true),
                new ProcessRankStep(isRequired: true),
                new ProcessFilterStep(new RecordPickerOptions(){MinScoreSimilarity = 0.3d, Take = 10, OrderByScoreDescending = true}, isRequired: true),
                new PostProcessCheckStep(),
                new PromptBuilderStep(isRequired: true)
            };
            var pipeline = new PipelineModel(settings, steps);
            return pipeline;
        }
        private Kernel CreateKernel(string openAiApiKey, string geminiApiKey)
        {
            if (string.IsNullOrEmpty(openAiApiKey) || string.IsNullOrEmpty(geminiApiKey))
            {
                throw new ArgumentNullException(nameof(openAiApiKey));
            }
            var configs = new List<KernelServiceBaseConfig>
            {
                new (AIProviderEnum.OpenAI, "gpt-4o-mini", openAiApiKey, serviceId: OpenAiServiceId),
                new (AIProviderEnum.OpenAI, "text-embedding-3-small", openAiApiKey, serviceId: OpenAiServiceId, serviceType: KernelServiceEnum.EmbeddingGenerator),
                new (AIProviderEnum.GoogleGemini, "gemini-2.0-flash", geminiApiKey, serviceId: GeminiServiceId),
                new (AIProviderEnum.GoogleGemini, "gemini-embedding-001", geminiApiKey, serviceId: GeminiServiceId, serviceType: KernelServiceEnum.EmbeddingGenerator)
            };
            var kernel = _semanticKernelManager.CreateKernel(configs);
            if (kernel.IsError)
            {
                throw new Exception("Kernel creation failed");
            }
            return kernel.Value;
        }

        private void AddGoogleKnowledgeBaseIfDefined(List<IKnowledgeBaseContainer> containers)
        {
            var googleApiKey = _configuration["SearchConfigurations:Google:ApiKey"];
            var googleEngine = _configuration["SearchConfigurations:Google:SearchEngineId"];
            if (!string.IsNullOrEmpty(googleApiKey) && !string.IsNullOrEmpty(googleEngine))
            {
                var request = new GoogleSearchRequest
                {
                    Settings = new GoogleSettings
                    {
                        SearchApiKey = googleApiKey,
                        Engine = googleEngine,
                        UseScraper = false,
                        AllowChunking = false,
                    },
                    Filter = new GoogleSearchFilterModel(),
                };
                containers.Add(new KnowledgeBaseContainer<KnowledgeUrlRecord, GoogleSearchFilterModel, GoogleSettings, UrlRecordFilter, UrlRecord>(_googleKB, request, null));
            }
        }
    }
}
