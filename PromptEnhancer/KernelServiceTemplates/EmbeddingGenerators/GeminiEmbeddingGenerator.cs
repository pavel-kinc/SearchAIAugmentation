using Microsoft.Extensions.DependencyInjection;

namespace PromptEnhancer.KernelServiceTemplates.EmbeddingGenerators
{
    /// <summary>
    /// Service template for adding Gemini Embedding Generator to the kernel's service collection.
    /// </summary>
    public class GeminiEmbeddingGenerator : KernelServiceTemplate
    {
        public string ModelId { get; }
        public string ApiKey { get; }
        public string? ServiceId { get; }

        public GeminiEmbeddingGenerator(string modelId, string apiKey, string? serviceId = null)
        {
            ModelId = modelId;
            ApiKey = apiKey;
            ServiceId = serviceId;
        }

        /// <inheritdoc/>
        public override IServiceCollection AddToServices(IServiceCollection services)
        {
            return services.AddGoogleAIEmbeddingGenerator(ModelId, ApiKey, serviceId: ServiceId);
        }
    }
}
