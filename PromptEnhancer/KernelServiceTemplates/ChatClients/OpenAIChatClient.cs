using Microsoft.Extensions.DependencyInjection;

namespace PromptEnhancer.KernelServiceTemplates.ChatClients
{
    /// <summary>
    /// Service template for adding an OpenAI Chat Client to the kernel's service collection.
    /// </summary>
    public class OpenAIChatClient : KernelServiceTemplate
    {
        public string ModelId { get; }
        public string ApiKey { get; }
        public string? ServiceId { get; }

        public OpenAIChatClient(string modelId, string apiKey, string? serviceId = null)
        {
            ModelId = modelId;
            ApiKey = apiKey;
            ServiceId = serviceId;
        }

        /// <inheritdoc/>
        public override IServiceCollection AddToServices(IServiceCollection services)
        {
            return services.AddOpenAIChatClient(ModelId, ApiKey, serviceId: ServiceId);
        }
    }
}
