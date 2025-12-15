using ErrorOr;
using Microsoft.Extensions.Logging;
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
        private readonly IKernelServiceFactory _kernelServiceFactory;

        // context plugins for semantic kernel creation (for later usage in steps)
        private readonly IEnumerable<ISemanticKernelContextPlugin> _contextPlugins;
        private readonly ILogger<SemanticKernelManager> _logger;

        public SemanticKernelManager(IKernelServiceFactory kernelServiceFactory, IEnumerable<ISemanticKernelContextPlugin> contextPlugins, ILogger<SemanticKernelManager> logger)
        {
            _kernelServiceFactory = kernelServiceFactory;
            _contextPlugins = contextPlugins;
            _logger = logger;
        }

        /// <inheritdoc/>
        public void AddPluginToSemanticKernel<Plugin>(Kernel kernel) where Plugin : class
        {
            _logger.LogInformation("Adding plugin {PluginName} to Semantic Kernel.", typeof(Plugin).Name);
            kernel.Plugins.AddFromType<Plugin>(typeof(Plugin).Name);
        }

        /// <inheritdoc/>
        public ErrorOr<Kernel> CreateKernel(IEnumerable<KernelServiceBaseConfig> kernelServiceConfigs, bool addInternalServices = false, bool addContextPlugins = true)
        {
            try
            {
                _logger.LogInformation("Creating Semantic Kernel with {ServiceCount} services. AddInternalServices: {AddInternalServices}, AddContextPlugins: {AddContextPlugins}", kernelServiceConfigs.Count(), addInternalServices, addContextPlugins);
                IKernelBuilder kernelBuilder = Kernel.CreateBuilder();
                var kernelServices = _kernelServiceFactory.CreateKernelServicesConfig(kernelServiceConfigs);
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
                _logger.LogError(ex, "Failed to create Semantic Kernel.");
                return Error.Failure($"{nameof(CreateKernel)}: failed kernel creation. - {ex.Message}");
            }

        }

        /// <inheritdoc/>
        public ErrorOr<IEnumerable<KernelServiceBaseConfig>> ConvertConfig(KernelConfiguration kernelData)
        {
            try
            {
                _logger.LogInformation("Converting KernelConfiguration to KernelServiceBaseConfig.");
                if (kernelData.Model is null || kernelData.AIApiKey is null)
                {
                    _logger.LogError("Failed to convert KernelConfiguration to KernelServiceBaseConfig. Model or Api key is null");
                    return Error.Failure($"{nameof(ConvertConfig)} failed");
                }
                var configs = new List<KernelServiceBaseConfig>
                {
                    new(kernelData.Provider, kernelData.Model!, kernelData.AIApiKey!, kernelData.DeploymentName, serviceId: kernelData.ClientServiceId)
                };
                // create embedding with same LLM config
                if (!string.IsNullOrWhiteSpace(kernelData.EmbeddingModel) && kernelData.UseLLMConfigForEmbeddings)
                {
                    configs.Add(new KernelServiceBaseConfig(kernelData.Provider, kernelData.EmbeddingModel!, kernelData.AIApiKey!, serviceId: kernelData.GeneratorServiceId, serviceType: KernelServiceEnum.EmbeddingGenerator));
                }
                // create embedding with its own config
                if (!string.IsNullOrWhiteSpace(kernelData.EmbeddingModel) && !kernelData.UseLLMConfigForEmbeddings && kernelData.EmbeddingProvider is not null)
                {
                    configs.Add(new KernelServiceBaseConfig((AIProviderEnum)kernelData.EmbeddingProvider, kernelData.EmbeddingModel!, kernelData.EmbeddingKey!, serviceId: kernelData.GeneratorServiceId, serviceType: KernelServiceEnum.EmbeddingGenerator));
                }
                return configs;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to convert KernelConfiguration to KernelServiceBaseConfig.");
                return Error.Failure($"{nameof(ConvertConfig)} failed", ex.Message);
            }

        }
    }
}
