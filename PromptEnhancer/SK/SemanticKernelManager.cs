using Azure.Core.Extensions;
using Google.Apis.CustomSearchAPI.v1.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Data;
using PromptEnhancer.Models;
using PromptEnhancer.Models.Configurations;
using PromptEnhancer.Search;

namespace PromptEnhancer.SK
{
    public static class SemanticKernelManager
    {
        public static Kernel? CreateKernel(KernelConfiguration kernelData)
        {
            if (kernelData.Provider == AIProviderEnum.OpenAI)
            {
                IKernelBuilder kernelBuilder = Kernel.CreateBuilder();
                kernelBuilder.AddOpenAIChatCompletion(
                        modelId: kernelData.Model,
                        apiKey: kernelData.AIApiKey);
                Kernel kernel = kernelBuilder.Build();
                return kernel;
            }
            else if (kernelData.Provider == AIProviderEnum.GoogleGemini)
            {
                IKernelBuilder kernelBuilder = Kernel.CreateBuilder();
#pragma warning disable SKEXP0070
                kernelBuilder.AddGoogleAIGeminiChatCompletion(
                        modelId: kernelData.Model,
                        apiKey: kernelData.AIApiKey);
                Kernel kernel = kernelBuilder.Build();
#pragma warning restore SKEXP0070
                return kernel;
            }
            return null;
        }

        public async static Task<ChatCompletionResult> GetAICompletionResult(Kernel kernel, string prompt)
        {
            if(prompt.Length > 1000)
            {
                throw new Exception("Prompt length exceeds limit.");
            }
            OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new()
            {
                FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
            };
            var chatCompletionService = kernel.Services.GetRequiredService<IChatCompletionService>();
            //var result = await chatCompletionService.GetChatMessageContentAsync(
            //    prompt,
            //    openAIPromptExecutionSettings,
            //    kernel
            //    );
            //TODO: handle other providers - response is specifig to openai - no built in abstraction from .net it seems
            //var replyInnerContent = result.InnerContent as OpenAI.Chat.ChatCompletion;
            return new ChatCompletionResult
            {
                AIOutput = null,//result?.Content,
                TokensUsed = 0//replyInnerContent?.Usage.TotalTokenCount ?? 0,
            };
        }

        public static async Task<ResultModel?> ProcessConfiguration(EnhancerConfiguration config)
        {
            //TODO move to EnhancerService probably
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
            resultView.Prompt = BuildPrompt(resultView, promptConf);
            var kernel = CreateKernel(kernelData!);
            resultView.AIResult = await GetAICompletionResult(kernel!, resultView.Prompt);
            resultView.AIResult.UsedURLs = usedUrls;
            return resultView;
        }

        private static string BuildPrompt(ResultModel resultView, PromptConfiguration promptConf)
        {
            //TODO build prompt better, this is temporary
            return @$"""
                        System: {promptConf.SystemInstructions}.
                        Generated output should concise of about {promptConf.TargetOutputLength} words.
                        {(!string.IsNullOrEmpty(promptConf.MacroDefinition) ? $" Any phrase wrapped in {promptConf.MacroDefinition} is a macro and must be kept exactly as-is." : string.Empty)}
                        Used Search Query: {resultView.Query}
                        Augmented Data: {resultView.SearchResult}
                        {(!string.IsNullOrEmpty(promptConf.AdditionalInstructions) ? promptConf.AdditionalInstructions : string.Empty)}
                    """;
        }
    }
}
