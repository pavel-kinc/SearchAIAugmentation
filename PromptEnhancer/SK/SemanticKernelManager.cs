using ErrorOr;
using Microsoft.Extensions.DependencyInjection;
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

        public ErrorOr<Kernel> CreateKernel(IEnumerable<KernelServiceBaseConfig> kernelServiceConfigs, bool addInternalServices = false, bool addContextPlugins = false)
        {
            try
            {
                var factory = KernelServiceFactory ?? new KernelServiceFactory();
                IKernelBuilder kernelBuilder = Kernel.CreateBuilder();
                // TODO, maybe just send in the service templates instead of this factory (getting rid of the dictionary)
                // maybe the config is uniform way for template creation? but then again, wouldnt it be easier just to create the templates directly?
                // think of advantages of this "factory"
                var kernelServices = factory.CreateKernelServicesConfig(kernelServiceConfigs);
                kernelBuilder.Services.AddKernelServices(kernelServices);
                if (addInternalServices)
                {
                    kernelBuilder.Services.AddInternalServices();
                }
                var kernel = kernelBuilder.Build();
                if (addContextPlugins)
                {
                    foreach (var plugin in kernel.Services.GetServices<ISemanticKernelContextPlugin>())
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
                    new(kernelData.Provider, kernelData.Model!, kernelData.AIApiKey!, kernelData.DeploymentName)
                    };
                if (!string.IsNullOrWhiteSpace(kernelData.EmbeddingModel) && kernelData.UseLLMConfigForEmbeddings)
                {
                    configs.Add(new KernelServiceBaseConfig(kernelData.Provider, kernelData.EmbeddingModel!, kernelData.AIApiKey!, serviceType: KernelServiceEnum.EmbeddingGenerator));
                }
                if (!string.IsNullOrWhiteSpace(kernelData.EmbeddingModel) && !kernelData.UseLLMConfigForEmbeddings && kernelData.EmbeddingProvider is not null)
                {
                    configs.Add(new KernelServiceBaseConfig((AIProviderEnum)kernelData.EmbeddingProvider, kernelData.EmbeddingModel!, kernelData.EmbeddingKey!, serviceType: KernelServiceEnum.EmbeddingGenerator));
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
