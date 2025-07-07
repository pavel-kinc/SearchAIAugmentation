using Newtonsoft.Json;
using PromptEnhancer.ChunkUtilities;
using PromptEnhancer.CustomJsonResolver;
using PromptEnhancer.Models;
using PromptEnhancer.Models.Configurations;
using PromptEnhancer.Prompt;
using PromptEnhancer.Search;
using PromptEnhancer.SK;
using System.Text;

namespace PromptEnhancer.Services
{
    public class EnhancerService : IEnhancerService
    {
        public EnhancerConfiguration CreateDefaultConfiguration(string? aiApiKey = null, AIProviderEnum aiProvider = AIProviderEnum.OpenAI, string aiModel = "gpt-4o-mini", string? searchApiKey = null, SearchProviderEnum searchProvider = SearchProviderEnum.Google, string? searchEngine = null)
        {
            var enhancerConfiguration = new EnhancerConfiguration();
            enhancerConfiguration.KernelConfiguration = new KernelConfiguration
            {
                AIApiKey = aiApiKey,
                Model = aiModel,
                Provider = aiProvider,
            };

            enhancerConfiguration.SearchConfiguration.SearchProviderData = new SearchProviderData
            {
                SearchApiKey = searchApiKey,
                Engine = searchEngine,
                Provider = searchProvider,
            };
            return enhancerConfiguration;
        }

        public async Task DownloadConfiguration(EnhancerConfiguration configuration, string filePath = "config.json", bool hideSecrets = true)
        {
            var json = GetConfigurationJson(configuration, hideSecrets);
            await File.WriteAllTextAsync("filePath", json);
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

        public async Task<ResultModel?> ProcessConfiguration(EnhancerConfiguration config)
        {
            var resultView = new ResultModel();
            var kernelData = config.KernelConfiguration;
            var searchConf = config.SearchConfiguration;
            var searchData = searchConf?.SearchProviderData;
            var promptConf = config.PromptConfiguration;

            resultView.Query = searchConf?.QueryString;

            // will be needed to search by params/config
            var textSearch = SearchProviderManager.CreateTextSearch(searchData!)!;
            var res = await SearchProviderManager.GetSearchResults(textSearch, searchConf!.QueryString!);
            var searchResults = await res.Results.ToListAsync();
            var usedUrls = searchResults.Where(x => !string.IsNullOrEmpty(x.Link)).Select(x => x.Link!);
            //temporary
            var useScraper = true;
            if (useScraper)
            {
                //will be needed some specifications from config what to search for maybe?
                var rawScrapedContent = await SearchWebScraper.ScrapeDataFromUrlsAsync(usedUrls);
                var chunks = ChunkGenerator.GenerateChunksFromData(rawScrapedContent);
                resultView.SearchResult = ChunkRanker.ExtractRelevantDataFromChunks(chunks, resultView.Query!);
            }
            else
            {
                //this uses snippets from search only
                resultView.SearchResult = string.Join('\n', searchResults.Select(x => x.Value));
            }
            resultView.Prompt = PromptUtility.BuildPrompt(resultView, promptConf);
            var kernel = SemanticKernelManager.CreateKernel(kernelData!);
            resultView.AIResult = await SemanticKernelManager.GetAICompletionResult(kernel!, resultView.Prompt);
            resultView.AIResult.UsedURLs = usedUrls;
            return resultView;
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
