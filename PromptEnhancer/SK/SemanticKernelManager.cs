using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

using PromptEnhancer.Models;
using PromptEnhancer.Models.Configurations;

namespace PromptEnhancer.SK
{
    public static class SemanticKernelManager
    {
        public static int MaxPromptLength = 3000;
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

        public async static Task<ChatCompletionResult> GetAICompletionResult(Kernel kernel, string prompt, int? maxPromptLength = null)
        {
            if (prompt.Length > (maxPromptLength ?? MaxPromptLength))
            {
                throw new Exception("Prompt length exceeds limit.");
            }
            OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new()
            {
                FunctionChoiceBehavior = FunctionChoiceBehavior.None()
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
    }
}
