using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using PromptEnhancer.Models;
using PromptEnhancer.Models.Configurations;
using PromptEnhancer.Models.Enums;
using PromptEnhancer.SK.Interfaces;

namespace PromptEnhancer.SK
{
    public class SemanticKernelManager : ISemanticKernelManager
    {
        private readonly int MaxPromptLength = 3000;

        public void AddPluginToSemanticKernel<Plugin>(Kernel kernel) where Plugin : class
        {
            kernel.Plugins.AddFromType<Plugin>(typeof(Plugin).Name);
        }

        public Kernel? CreateKernel(KernelConfiguration kernelData)
        {
            if (kernelData.Provider == AIProviderEnum.OpenAI)
            {
                IKernelBuilder kernelBuilder = Kernel.CreateBuilder();
                kernelBuilder.AddOpenAIChatCompletion(
                        modelId: kernelData.Model!,
                        apiKey: kernelData.AIApiKey!);
#pragma warning disable SKEXP0010
                kernelBuilder.AddOpenAIEmbeddingGenerator(
                        modelId: "text-embedding-3-small",
                        apiKey: kernelData.AIApiKey!);
#pragma warning restore SKEXP0010
                Kernel kernel = kernelBuilder.Build();
                return kernel;
            }
            else if (kernelData.Provider == AIProviderEnum.GoogleGemini)
            {
                IKernelBuilder kernelBuilder = Kernel.CreateBuilder();
#pragma warning disable SKEXP0070
                kernelBuilder.AddGoogleAIGeminiChatCompletion(
                        modelId: kernelData.Model!,
                        apiKey: kernelData.AIApiKey!);
                kernelBuilder.AddGoogleAIEmbeddingGenerator(
                        modelId: kernelData.Model!,
                        apiKey: kernelData.AIApiKey!);
                Kernel kernel = kernelBuilder.Build();
#pragma warning restore SKEXP0070
                return kernel;
            }
            return null;
        }

        public async Task<ChatCompletionResult> GetAICompletionResult(Kernel kernel, string prompt, int? maxPromptLength = null)
        {
            if (prompt.Length > (maxPromptLength ?? MaxPromptLength))
            {
                throw new Exception("Prompt length exceeds limit.");
            }
            PromptExecutionSettings promptExecutionSettings = new()
            {
                FunctionChoiceBehavior = FunctionChoiceBehavior.None()
            };
            var chatCompletionService = kernel.Services.GetRequiredService<IChatCompletionService>();
            var result = await chatCompletionService.GetChatMessageContentAsync(
                prompt,
                promptExecutionSettings,
                kernel
                );
            //TODO: handle other providers - response is specifig to openai - no built in abstraction from .net it seems
            var replyInnerContent = result.InnerContent as OpenAI.Chat.ChatCompletion;
            return new ChatCompletionResult
            {
                AIOutput = result?.Content,
                TokensUsed = replyInnerContent?.Usage.TotalTokenCount ?? 0,
            };
        }
    }
}
