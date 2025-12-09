using ErrorOr;
using Microsoft.SemanticKernel;
using PromptEnhancer.Extensions;
using PromptEnhancer.Models.Configurations;
using PromptEnhancer.Models.Enums;
using PromptEnhancer.Plugins.Interfaces;
using PromptEnhancer.SK.Interfaces;

namespace PromptEnhancer.SK
{
    /// <summary>
    /// Manages the creation and configuration of semantic kernels, including the addition of plugins and conversion of
    /// kernel configurations.
    /// </summary>
    /// <remarks>This class provides methods to create and configure semantic kernels using specified services
    /// and plugins. It also supports converting kernel configuration data into a format suitable for kernel
    /// initialization. The manager relies on an <see cref="IKernelServiceFactory"/> to create kernel services and can
    /// optionally add internal services and context plugins during kernel creation.</remarks>
    public class SemanticKernelManager : ISemanticKernelManager
    {
        // context plugins for semantic kernel creation (for later usage in steps)
        private readonly IEnumerable<ISemanticKernelContextPlugin> _contextPlugins;

        public IKernelServiceFactory KernelServiceFactory { get; }

        public SemanticKernelManager(IKernelServiceFactory kernelServiceFactory, IEnumerable<ISemanticKernelContextPlugin> contextPlugins)
        {
            KernelServiceFactory = kernelServiceFactory;
            _contextPlugins = contextPlugins;
        }

        /// <inheritdoc/>
        public void AddPluginToSemanticKernel<Plugin>(Kernel kernel) where Plugin : class
        {
            kernel.Plugins.AddFromType<Plugin>(typeof(Plugin).Name);
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
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
