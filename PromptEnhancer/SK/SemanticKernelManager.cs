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

        public async static Task<string> GetAICompletionResult(Kernel kernel, string prompt)
        {
            OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new()
            {
                FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
            };
            var chatCompletionService = kernel.Services.GetRequiredService<IChatCompletionService>();
            var result = await chatCompletionService.GetChatMessageContentAsync(
                prompt,
                openAIPromptExecutionSettings,
                kernel
                );
            return result?.Content!;
        }

        public static async Task<ResultModel?> ProcessConfiguration(EnhancerConfiguration config)
        {
            var resultView = new ResultModel();
            var kernelData = config.KernelConfiguration;
            var searchConf = config.SearchConfiguration;
            var searchData = searchConf?.SearchProviderData;
            var promptConf = config.PromptConfiguration;

            resultView.Query = searchConf?.QueryString;

            var textSearch = SearchProviderManager.CreateTextSearch(searchData!)!;
            var res = await SearchProviderManager.GetSearchResults(textSearch, searchConf!.QueryString!);
            var results = await res.Results.ToListAsync();
            resultView.SearchResult = string.Join('\n', results.Select(x => x.Value));
            //TODO build prompt
            resultView.Prompt = @$"""System: {promptConf.SystemInstructions}.
                                Generated output should concise of about {promptConf.TargetOutputLength} words.
                                {(!string.IsNullOrEmpty(promptConf.MacroDefinition) ? $" Any phrase wrapped in {promptConf.MacroDefinition} is a macro and must be kept exactly as-is." : string.Empty)}
                                Used Search Query: {resultView.Query}
                                Augmented Data: {resultView.SearchResult}""
                                {(!string.IsNullOrEmpty(promptConf.AdditionalInstructions) ? promptConf.AdditionalInstructions : string.Empty)}";
            var kernel = CreateKernel(kernelData!);
            resultView.AIResult = await GetAICompletionResult(kernel!, resultView.Prompt);
            return resultView;
        }
    }
}
