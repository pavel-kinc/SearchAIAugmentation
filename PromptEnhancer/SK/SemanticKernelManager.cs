using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using PromptEnhancer.Extensions;
using PromptEnhancer.Models;
using PromptEnhancer.Models.Configurations;
using PromptEnhancer.Models.Enums;
using PromptEnhancer.Plugins.Interfaces;
using PromptEnhancer.SK.Interfaces;

namespace PromptEnhancer.SK
{
    public class SemanticKernelManager : ISemanticKernelManager
    {
        private readonly int MaxPromptLength = 3000;

        public IKernelServiceFactory KernelServiceFactory { get; }

        public SemanticKernelManager(IKernelServiceFactory kernelServiceFactory)
        {
            KernelServiceFactory = kernelServiceFactory;
        }

        public void AddPluginToSemanticKernel<Plugin>(Kernel kernel) where Plugin : class
        {
            kernel.Plugins.AddFromType<Plugin>(typeof(Plugin).Name);
        }

        public Kernel? CreateKernel(KernelConfiguration kernelData)
        {
            var factory = KernelServiceFactory ?? new KernelServiceFactory();
            IKernelBuilder kernelBuilder = Kernel.CreateBuilder();
            // TODO, maybe just send in the service templates instead of this factory (getting rid of the dictionary)
            // maybe the config is uniform way for template creation? but then again, wouldnt it be easier just to create the templates directly?
            // think of advantages of this "factory"
            var kernelServices = factory.CreateKernelServicesConfig(ConvertConfig(kernelData));
            kernelBuilder.Services.AddKernelServices(kernelServices);
            kernelBuilder.Services.AddInternalServices();
            var kernel = kernelBuilder.Build();
            foreach (var plugin in kernel.Services.GetServices<ISemanticKernelPlugin>())
            {
                kernel.Plugins.AddFromObject(plugin, plugin.GetType().Name);
            }
            //kernelBuilder.Plugins.AddFromType<DateTimePlugin>(typeof(DateTimePlugin).Name);
            return kernel;
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

        private IEnumerable<KernelServiceBaseConfig> ConvertConfig(KernelConfiguration kernelData)
        {
            var configs = new List<KernelServiceBaseConfig>
            {
                new(kernelData.Provider, kernelData.Model!, kernelData.AIApiKey!)
            };
            if (!string.IsNullOrWhiteSpace(kernelData.EmbeddingModel))
            {
                configs.Add(new KernelServiceBaseConfig(kernelData.Provider, kernelData.Model!, kernelData.AIApiKey!, serviceType: KernelServiceEnum.EmbeddingGenerator));
            }
            return configs;
        }
    }
}
