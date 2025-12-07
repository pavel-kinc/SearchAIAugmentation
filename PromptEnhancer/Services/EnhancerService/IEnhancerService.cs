using ErrorOr;
using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel;
using PromptEnhancer.KnowledgeBaseCore;
using PromptEnhancer.KnowledgeBaseCore.Interfaces;
using PromptEnhancer.KnowledgeRecord;
using PromptEnhancer.KnowledgeSearchRequest.Examples;
using PromptEnhancer.Models;
using PromptEnhancer.Models.Configurations;
using PromptEnhancer.Models.Enums;
using PromptEnhancer.Models.Pipeline;
using PromptEnhancer.Pipeline.Interfaces;

namespace PromptEnhancer.Services.EnhancerService
{
    public interface IEnhancerService
    {
        public Task<ErrorOr<IList<ResultModel>>> ProcessConfiguration(EnhancerConfiguration config, IEnumerable<Entry> entries, Kernel? kernel = null, CancellationToken cancellationToken = default);
        public Task<ErrorOr<IList<ResultModel>>> ProcessPipelineAsync(PipelineModel pipeline, IEnumerable<PipelineRun> entries, CancellationToken cancellationToken = default);
        public ErrorOr<PipelineSettings> CreatePipelineSettingsFromConfig(PromptConfiguration promptConf, PipelineAdditionalSettings pipelineSettings, KernelConfiguration? kernelData = null, Kernel? kernel = null);
        public IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponse(PipelineSettings settings, PipelineRun context, CancellationToken ct = default);
        public EnhancerConfiguration CreateDefaultConfiguration(string? aiApiKey = null, AIProviderEnum aiProvider = AIProviderEnum.OpenAI, string aiModel = "gpt-4o-mini", string embeddingModel = "text-embedding-3-small");

        public IKnowledgeBaseContainer CreateDefaultDataContainer<TModel>(IEnumerable<TModel> data) where TModel : class;
        public IKnowledgeBaseContainer CreateDefaultDataContainer<TRecord, TModel>(IEnumerable<TModel> data) where TModel : class where TRecord : KnowledgeRecord<TModel>, new();

        public ErrorOr<PipelineModel> CreateDefaultSearchPipeline(IEnumerable<IKnowledgeBaseContainer> containers, PromptConfiguration? promptConf = null, PipelineAdditionalSettings? pipelineSettings = null, KernelConfiguration? kernelData = null, Kernel? kernel = null);

        public ErrorOr<PipelineModel> CreateDefaultSearchPipelineWithoutGenerationStep(IEnumerable<IKnowledgeBaseContainer> containers, PromptConfiguration? promptConf = null, PipelineAdditionalSettings? pipelineSettings = null, KernelConfiguration? kernelData = null, Kernel? kernel = null);

        public IEnumerable<IPipelineStep> CreateDefaultGoogleSearchPipelineSteps(string googleApiKey, string googleEngine, GoogleSearchFilterModel? searchFilter = null, GoogleSettings? googleSettings = null, UrlRecordFilter? filter = null, bool useScraper = false);

        public Task DownloadConfiguration(EnhancerConfiguration configuration, string filePath = "config.json", bool hideSecrets = true);
        public byte[] ExportConfigurationToBytes(EnhancerConfiguration configuration, bool hideSecrets = true);

        public Task<EnhancerConfiguration?> ImportConfigurationFromFile(string filePath);

        public EnhancerConfiguration? ImportConfigurationFromBytes(byte[] jsonBytes);
    }
}
