using Microsoft.Extensions.DependencyInjection;
using OllamaSharp;

namespace PromptEnhancer.KernelServiceTemplates.EmbeddingGenerators
{
    /// <summary>
    /// Service template for adding an Ollama Embedding Generator to the kernel's service collection.
    /// </summary>
    public class OllamaEmbeddingGenerator : KernelServiceTemplate
    {
        public string Uri { get; }
        public string Model { get; }
        public string? ServiceId { get; }

        public OllamaEmbeddingGenerator(string uri, string model = "", string? serviceId = null)
        {
            Uri = uri;
            Model = model;
            ServiceId = serviceId;
        }

        /// <inheritdoc/>
        public override IServiceCollection AddToServices(IServiceCollection services)
        {
            return services.AddOllamaEmbeddingGenerator(new OllamaApiClient(Uri, Model), serviceId: ServiceId);
        }
    }
}
