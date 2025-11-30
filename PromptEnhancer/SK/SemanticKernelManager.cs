using ErrorOr;
using Microsoft.SemanticKernel;
using PromptEnhancer.Extensions;
using PromptEnhancer.Models.Configurations;
using PromptEnhancer.Models.Enums;
using PromptEnhancer.Plugins.Interfaces;
using PromptEnhancer.SK.Interfaces;

namespace PromptEnhancer.SK
{
    public class SemanticKernelManager : ISemanticKernelManager
    {
        private readonly IEnumerable<ISemanticKernelContextPlugin> _contextPlugins;

        public IKernelServiceFactory KernelServiceFactory { get; }

        public SemanticKernelManager(IKernelServiceFactory kernelServiceFactory, IEnumerable<ISemanticKernelContextPlugin> contextPlugins)
        {
            KernelServiceFactory = kernelServiceFactory;
            _contextPlugins = contextPlugins;
        }

        public void AddPluginToSemanticKernel<Plugin>(Kernel kernel) where Plugin : class
        {
            kernel.Plugins.AddFromType<Plugin>(typeof(Plugin).Name);
        }

        public ErrorOr<Kernel> CreateKernel(IEnumerable<KernelServiceBaseConfig> kernelServiceConfigs, bool addInternalServices = false, bool addContextPlugins = true)
        {
            try
            {
                var factory = KernelServiceFactory ?? new KernelServiceFactory();
                IKernelBuilder kernelBuilder = Kernel.CreateBuilder();
                var kernelServices = factory.CreateKernelServicesConfig(kernelServiceConfigs);
                kernelBuilder.Services.AddKernelServices(kernelServices);
                if (addInternalServices)
                {
                    kernelBuilder.Services.AddInternalServices();
                }
                var kernel = kernelBuilder.Build();
                if (addContextPlugins)
                {
                    foreach (var plugin in _contextPlugins)
                    {
                        kernel.Plugins.AddFromObject(plugin, plugin.GetType().Name);
                    }
                }
                return kernel;
            }
            catch (Exception ex)
            {
                return Error.Failure($"{nameof(CreateKernel)}: failed kernel creation. - {ex.Message}");
            }

        }

        //public async Task<ChatCompletionResult> GetAICompletionResult(Kernel kernel, string prompt, int? maxPromptLength = null)
        //{
        //    if (prompt.Length > (maxPromptLength ?? MaxPromptLength))
        //    {
        //        throw new Exception("Prompt length exceeds limit.");
        //    }
        //    PromptExecutionSettings promptExecutionSettings = new()
        //    {
        //        FunctionChoiceBehavior = FunctionChoiceBehavior.None()
        //    };
        //    var chatCompletionService = kernel.Services.GetRequiredService<IChatCompletionService>();
        //    var result = await chatCompletionService.GetChatMessageContentAsync(
        //        prompt,
        //        promptExecutionSettings,
        //        kernel
        //        );
        //    //TODO: handle other providers - response is specifig to openai - no built in abstraction from .net it seems
        //    var replyInnerContent = result.InnerContent as OpenAI.Chat.ChatCompletion;
        //    return new ChatCompletionResult
        //    {
        //        AIOutput = result?.Content,
        //        TokensUsed = replyInnerContent?.Usage.TotalTokenCount ?? 0,
        //    };
        //}

        public ErrorOr<IEnumerable<KernelServiceBaseConfig>> ConvertConfig(KernelConfiguration kernelData)
        {
            try
            {
                var configs = new List<KernelServiceBaseConfig>
                {
                    new(kernelData.Provider, kernelData.Model!, kernelData.AIApiKey!, kernelData.DeploymentName, serviceId: kernelData.ClientServiceId)
                };
                if (!string.IsNullOrWhiteSpace(kernelData.EmbeddingModel) && kernelData.UseLLMConfigForEmbeddings)
                {
                    configs.Add(new KernelServiceBaseConfig(kernelData.Provider, kernelData.EmbeddingModel!, kernelData.AIApiKey!, serviceId: kernelData.GeneratorServiceId, serviceType: KernelServiceEnum.EmbeddingGenerator));
                }
                if (!string.IsNullOrWhiteSpace(kernelData.EmbeddingModel) && !kernelData.UseLLMConfigForEmbeddings && kernelData.EmbeddingProvider is not null)
                {
                    configs.Add(new KernelServiceBaseConfig((AIProviderEnum)kernelData.EmbeddingProvider, kernelData.EmbeddingModel!, kernelData.EmbeddingKey!, serviceId: kernelData.GeneratorServiceId, serviceType: KernelServiceEnum.EmbeddingGenerator));
                }
                return configs;
            }
            catch (Exception ex)
            {
                return Error.Failure($"{nameof(ConvertConfig)} failed", ex.Message);
            }

        }
    }
}
