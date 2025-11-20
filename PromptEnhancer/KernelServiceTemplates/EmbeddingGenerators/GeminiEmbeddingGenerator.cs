using Microsoft.Extensions.DependencyInjection;

namespace PromptEnhancer.KernelServiceTemplates.EmbeddingGenerators
{
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

        public override IServiceCollection AddToServices(IServiceCollection services)
        {
            return services.AddGoogleAIEmbeddingGenerator(ModelId, ApiKey, serviceId: ServiceId);
        }
    }
}
