using Microsoft.Extensions.Logging;
using PromptEnhancer.KernelServiceTemplates;
using PromptEnhancer.KernelServiceTemplates.ChatClients;
using PromptEnhancer.KernelServiceTemplates.EmbeddingGenerators;
using PromptEnhancer.Models.Configurations;
using PromptEnhancer.Models.Enums;
using PromptEnhancer.SK.Interfaces;

namespace PromptEnhancer.SK
{
    /// <summary>
    /// Provides a factory for creating kernel service instances based on specified configurations.
    /// </summary>
    /// <remarks>This factory supports multiple AI providers and kernel service types, including OpenAI, Azure
    /// OpenAI,  Google Gemini, Ollama, and ONNX. It uses a predefined mapping of AI providers and service types to 
    /// corresponding factory methods for creating instances of <see cref="IKernelServiceTemplate"/>.</remarks>
    public class KernelServiceFactory : IKernelServiceFactory
    {
        public KernelServiceFactory(ILogger<KernelServiceFactory> logger)
        {
            _logger = logger;
        }
        private static readonly Dictionary<(AIProviderEnum, KernelServiceEnum), Func<KernelServiceBaseConfig, IKernelServiceTemplate>> Factories = new()
        {
            // OpenAI
            [(AIProviderEnum.OpenAI, KernelServiceEnum.ChatClient)] =
            cfg => new OpenAIChatClient(cfg.Model, cfg.Key, cfg.ServiceId),

            [(AIProviderEnum.OpenAI, KernelServiceEnum.EmbeddingGenerator)] =
            cfg => new OpenAIEmbeddingGenerator(cfg.Model, cfg.Key, cfg.ServiceId),

            // Azure OpenAI
            [(AIProviderEnum.AzureOpenAI, KernelServiceEnum.ChatClient)] =
            cfg => new AzureOpenAIChatClient(cfg.DeploymentName!, cfg.Model, cfg.Key, cfg.ServiceId),

            [(AIProviderEnum.AzureOpenAI, KernelServiceEnum.EmbeddingGenerator)] =
            cfg => new AzureOpenAIEmbeddingGenerator(cfg.DeploymentName!, cfg.Model, cfg.Key, cfg.ServiceId),

            // Google Gemini
            [(AIProviderEnum.GoogleGemini, KernelServiceEnum.ChatClient)] =
            cfg => new GeminiChatClient(cfg.Model, cfg.Key, cfg.ServiceId),

            [(AIProviderEnum.GoogleGemini, KernelServiceEnum.EmbeddingGenerator)] =
            cfg => new GeminiEmbeddingGenerator(cfg.Model, cfg.Key, cfg.ServiceId),

            // Ollama
            [(AIProviderEnum.Ollama, KernelServiceEnum.ChatClient)] =
            cfg => new OllamaChatClient(cfg.Key, cfg.Model, cfg.ServiceId),

            [(AIProviderEnum.Ollama, KernelServiceEnum.EmbeddingGenerator)] =
            cfg => new OllamaEmbeddingGenerator(cfg.Key, cfg.Model, cfg.ServiceId),

            // ONNX
            [(AIProviderEnum.Onnx, KernelServiceEnum.ChatClient)] =
            cfg => new OnnxChatClient(cfg.Model, cfg.Key, cfg.ServiceId),

            [(AIProviderEnum.Onnx, KernelServiceEnum.EmbeddingGenerator)] =
            cfg => new OnnxEmbeddingGenerator(cfg.Model, cfg.Key, cfg.ServiceId),
        };
        private readonly ILogger<KernelServiceFactory> _logger;

        /// <inheritdoc/>
        public virtual IEnumerable<IKernelServiceTemplate> CreateKernelServicesConfig(IEnumerable<KernelServiceBaseConfig> configs)
        {
            var serviceTemplates = new List<IKernelServiceTemplate>();
            foreach (var config in configs)
            {
                AddToServiceTemplatesIfValid(config, serviceTemplates);
            }
            _logger.LogInformation("Created {ServiceCount} kernel service templates from {ConfigCount} configurations.", serviceTemplates.Count, configs.Count());
            return serviceTemplates;
        }

        /// <summary>
        /// Adds a service template to the specified collection if the provided configuration is valid.
        /// </summary>
        /// <param name="config">The configuration object containing the service provider, service type, and key information.</param>
        /// <param name="serviceTemplates">The collection to which the service template will be added if the configuration is valid.</param>
        private void AddToServiceTemplatesIfValid(KernelServiceBaseConfig config, List<IKernelServiceTemplate> serviceTemplates)
        {
            var key = (config.KernelServiceProvider, config.KernelServiceType);
            if (Factories.TryGetValue(key, out var factory) && !string.IsNullOrWhiteSpace(config.Key))
            {
                _logger.LogInformation("Creating kernel service template for provider: {Provider}, service type: {ServiceType}.", config.KernelServiceProvider, config.KernelServiceType);
                serviceTemplates.Add(factory(config));
            }
            else
            {
                _logger.LogWarning("No factory found for provider: {Provider}, service type: {ServiceType}, or the API key is missing.", config.KernelServiceProvider, config.KernelServiceType);
            }
        }
    }
}
