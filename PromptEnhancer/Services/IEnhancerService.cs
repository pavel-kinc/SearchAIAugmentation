using PromptEnhancer.Models;
using PromptEnhancer.Models.Configurations;

namespace PromptEnhancer.Services
{
    public interface IEnhancerService
    {
        public Task<ResultModel?> ProcessConfiguration(EnhancerConfiguration config);
        public EnhancerConfiguration CreateDefaultConfiguration(string? aiApiKey = null, AIProviderEnum aiProvider = AIProviderEnum.OpenAI, string aiModel = "gpt-4o-mini", string? searchApiKey = null, SearchProviderEnum searchProvider = SearchProviderEnum.Google, string? searchEngine = null);

        public Task DownloadConfiguration(EnhancerConfiguration configuration, string filePath = "config.json", bool hideSecrets = true);
        public byte[] ExportConfigurationToBytes(EnhancerConfiguration configuration, bool hideSecrets = true);

        public Task<EnhancerConfiguration?> ImportConfigurationFromFile(string filePath);

        public EnhancerConfiguration? ImportConfigurationFromBytes(byte[] jsonBytes);
    }
}
