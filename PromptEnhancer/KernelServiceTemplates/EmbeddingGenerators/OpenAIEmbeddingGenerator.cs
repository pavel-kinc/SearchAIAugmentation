using Microsoft.Extensions.DependencyInjection;

namespace PromptEnhancer.KernelServiceTemplates.EmbeddingGenerators
{
    /// <summary>
    /// Service template for adding an OpenAI Embedding Generator to the kernel's service collection.
    /// </summary>
    public class OpenAIEmbeddingGenerator : KernelServiceTemplate
    {
        public string ModelId { get; }
        public string ApiKey { get; }
        public string? ServiceId { get; }

        public OpenAIEmbeddingGenerator(string modelId, string apiKey, string? serviceId = null)
        {
            ModelId = modelId;
            ApiKey = apiKey;
            ServiceId = serviceId;
        }

        /// <inheritdoc/>
        public override IServiceCollection AddToServices(IServiceCollection services)
        {
            return services.AddOpenAIEmbeddingGenerator(ModelId, ApiKey, serviceId: ServiceId);
        }
    }
}
