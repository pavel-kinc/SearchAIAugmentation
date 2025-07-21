using PromptEnhancer.Models;
using PromptEnhancer.Models.Configurations;
using PromptEnhancer.Models.Enums;

namespace PromptEnhancer.Services
{
    public interface IEnhancerService
    {
        public Task<IList<ResultModel>> ProcessConfiguration(EnhancerConfiguration config, IEnumerable<Entry> entries);
        public EnhancerConfiguration CreateDefaultConfiguration(string? aiApiKey = null, AIProviderEnum aiProvider = AIProviderEnum.OpenAI, string aiModel = "gpt-4o-mini", string? searchApiKey = null, SearchProviderEnum searchProvider = SearchProviderEnum.Google, string? searchEngine = null);

        public Task DownloadConfiguration(EnhancerConfiguration configuration, string filePath = "config.json", bool hideSecrets = true);
        public byte[] ExportConfigurationToBytes(EnhancerConfiguration configuration, bool hideSecrets = true);

        public Task<EnhancerConfiguration?> ImportConfigurationFromFile(string filePath);

        public EnhancerConfiguration? ImportConfigurationFromBytes(byte[] jsonBytes);
    }
}
