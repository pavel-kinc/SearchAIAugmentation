using ErrorOr;
using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel;
using PromptEnhancer.Models;
using PromptEnhancer.Models.Configurations;
using PromptEnhancer.Models.Enums;
using PromptEnhancer.Models.Pipeline;

namespace PromptEnhancer.Services.EnhancerService
{
    public interface IEnhancerService
    {
        public Task<ErrorOr<IList<ResultModel>>> ProcessConfiguration(EnhancerConfiguration config, IEnumerable<Entry> entries, Kernel? kernel = null, CancellationToken cancellationToken = default);
        public Task<ErrorOr<IList<ResultModel>>> ProcessPipelineAsync(PipelineModel pipeline, IEnumerable<PipelineContext> entries, CancellationToken cancellationToken = default);
        public ErrorOr<PipelineSettings> CreatePipelineSettingsFromConfig(PromptConfiguration promptConf, PipelineAdditionalSettings pipelineSettings, KernelConfiguration? kernelData = null, Kernel? kernel = null);
        IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponse(PipelineSettings settings, PipelineContext context, CancellationToken ct = default);
        public EnhancerConfiguration CreateDefaultConfiguration(string? aiApiKey = null, AIProviderEnum aiProvider = AIProviderEnum.OpenAI, string aiModel = "gpt-4o-mini", string embeddingModel = "text-embedding-3-small");

        public Task DownloadConfiguration(EnhancerConfiguration configuration, string filePath = "config.json", bool hideSecrets = true);
        public byte[] ExportConfigurationToBytes(EnhancerConfiguration configuration, bool hideSecrets = true);

        public Task<EnhancerConfiguration?> ImportConfigurationFromFile(string filePath);

        public EnhancerConfiguration? ImportConfigurationFromBytes(byte[] jsonBytes);
    }
}
