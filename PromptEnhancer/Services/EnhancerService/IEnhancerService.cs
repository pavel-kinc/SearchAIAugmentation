using Microsoft.SemanticKernel;
using PromptEnhancer.Models;
using PromptEnhancer.Models.Configurations;
using PromptEnhancer.Models.Enums;

namespace PromptEnhancer.Services.EnhancerService
{
    public interface IEnhancerService
    {
        public Task<IList<ResultModel>> ProcessConfiguration(EnhancerConfiguration config, IEnumerable<Entry> entries, Kernel? kernel = null);
        public EnhancerConfiguration CreateDefaultConfiguration(string? aiApiKey = null, AIProviderEnum aiProvider = AIProviderEnum.OpenAI, string aiModel = "gpt-4o-mini", string embeddingModel = "text-embedding-3-small", string? searchApiKey = null, SearchProviderEnum searchProvider = SearchProviderEnum.Google, string? searchEngine = null);

        public Task DownloadConfiguration(EnhancerConfiguration configuration, string filePath = "config.json", bool hideSecrets = true);
        public byte[] ExportConfigurationToBytes(EnhancerConfiguration configuration, bool hideSecrets = true);

        public Task<EnhancerConfiguration?> ImportConfigurationFromFile(string filePath);

        public EnhancerConfiguration? ImportConfigurationFromBytes(byte[] jsonBytes);
    }
}
