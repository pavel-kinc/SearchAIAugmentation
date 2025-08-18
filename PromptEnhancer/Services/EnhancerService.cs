using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Newtonsoft.Json;
using PromptEnhancer.ChunkUtilities;
using PromptEnhancer.ChunkUtilities.Interfaces;
using PromptEnhancer.CustomJsonResolver;
using PromptEnhancer.Models;
using PromptEnhancer.Models.Configurations;
using PromptEnhancer.Models.Enums;
using PromptEnhancer.Prompt;
using PromptEnhancer.Search.Interfaces;
using PromptEnhancer.Services.Interfaces;
using PromptEnhancer.SK;
using PromptEnhancer.SK.Interfaces;
using System.Collections.Concurrent;
using System.Text;
using System.Net.WebSockets;

namespace PromptEnhancer.Services
{
    public class EnhancerService : IEnhancerService
    {
        private readonly ISemanticKernelManager _semanticKernelManager;
        private readonly ISearchProviderManager _searchProviderManager;
        private readonly ISearchWebScraper _searchWebScraper;
        private readonly IChunkGenerator _chunkGenerator;
        private readonly IChunkRanker _chunkRanker;

        public EnhancerService(IChunkRanker chunkRanker, ISemanticKernelManager semanticKernelManager, ISearchProviderManager searchProviderManager, ISearchWebScraper searchWebScraper, IChunkGenerator chunkGenerator)
        {
            _chunkRanker = chunkRanker;
            _semanticKernelManager = semanticKernelManager;
            _searchProviderManager = searchProviderManager;
            _searchWebScraper = searchWebScraper;
            _chunkGenerator = chunkGenerator;
        }
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

        public async Task<IList<ResultModel>> ProcessConfiguration(EnhancerConfiguration config, IEnumerable<Entry> entries)
        {
            
            var kernelData = config.KernelConfiguration;
            var searchConf = config.SearchConfiguration;
            var searchData = searchConf?.SearchProviderData;
            var promptConf = config.PromptConfiguration;

            //change to list of results, deal with errors in result creation

            // will be needed to search by params/config
            // refactor into pipeline
            var kernel = _semanticKernelManager.CreateKernel(kernelData!);
            _semanticKernelManager.AddPluginToSemanticKernel<SemanticSlicerChunkGenerator>(kernel!);
            kernel!.Plugins.TryGetPlugin(typeof(SemanticSlicerChunkGenerator).Name, out var plugin);
            //var service = kernel.GetRequiredService<IChunkGenerator>();

            OpenAIPromptExecutionSettings settings = new()
            {
                FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
            };

            

            var prompt = "{{SemanticSlicerChunkGenerator.generate_chunks_from_string $rawText}}";
            var arg = "Multiple Implementations: If there are multiple implementations of generate_chunks_from_string, ensure that the correct one is invoked by specifying the appropriate function name or handling the selection logic.\n\nError Handling: Implement robust error handling to manage scenarios where the function might not be available or the invocation fails.\n\nFunction Signatures: Ensure that the function signatures match the expected parameters to avoid runtime errors.";
            var c = new PromptTemplateConfig
            {
                Template = prompt,
                TemplateFormat = PromptTemplateConfig.SemanticKernelTemplateFormat,
                Name = "ChunkGenTemplate"
            };
            var factory = new KernelPromptTemplateFactory();
            var b = factory.TryCreate(c, out var promptTemplate);
            var rendered = await promptTemplate.RenderAsync(kernel, new KernelArguments { { "rawText", arg } });
            var res = await kernel.InvokePromptAsync(prompt, new KernelArguments{ { "rawText", arg } });
            //var result = await kernel.InvokeAsync(typeof(SemanticSlicerChunkGenerator).Name, "generate_chunks_from_string", new KernelArguments{ { "rawText", arg } });
            //var result2 = await kernel.InvokePromptAsync($"Generate chunks from this string and return only list of strings: {arg}", new(settings));
            //var val1 = result.GetValue<IList<string>>();
            //var val = result2.GetValue<string>();
            var val = res.GetValue<string>();
            var textSearch = _searchProviderManager.CreateTextSearch(searchData!)!;

            var cb = new ConcurrentBag<ResultModel>();

            await Parallel.ForEachAsync(entries, async (entry, _) =>
            {
                var query = entry.QueryString!;
                var resultView = new ResultModel();
                var res = await _searchProviderManager.GetSearchResults(textSearch, query);
                var searchResults = await res.Results.ToListAsync();
                var usedUrls = searchResults.Where(x => !string.IsNullOrEmpty(x.Link)).Select(x => x.Link!);
                //temporary
                var useScraper = true;
                if (useScraper)
                {
                    //will be needed some specifications from config what to search for maybe?
                    var rawScrapedContent = await _searchWebScraper.ScrapeDataFromUrlsAsync(usedUrls);
                    var chunks = _chunkGenerator.GenerateChunksFromData(rawScrapedContent);
                    resultView.SearchResult = _chunkRanker.ExtractRelevantDataFromChunks(chunks, query);
                }
                else
                {
                    //this uses snippets from search only
                    resultView.SearchResult = string.Join('\n', searchResults.Select(x => x.Value));
                }
                resultView.Prompt = PromptUtility.BuildPrompt(promptConf, query, resultView.SearchResult);
                //resultView.AIResult = await _semanticKernelManager.GetAICompletionResult(kernel!, resultView.Prompt);
                resultView.AIResult.UsedURLs = usedUrls;
                cb.Add(resultView);
            });
            
            
            return [.. cb];
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
