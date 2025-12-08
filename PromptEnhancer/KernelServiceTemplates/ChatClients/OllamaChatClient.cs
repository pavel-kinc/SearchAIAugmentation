using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using OllamaSharp;

namespace PromptEnhancer.KernelServiceTemplates.ChatClients
{
    /// <summary>
    /// Service template for adding an Ollama Chat Client to the kernel's service collection.
    /// </summary>
    public class OllamaChatClient : KernelServiceTemplate
    {
        public string Uri { get; }
        public string Model { get; }
        public string? ServiceId { get; }

        public OllamaChatClient(string uri, string model = "", string? serviceId = null)
        {
            Uri = uri;
            Model = model;
            ServiceId = serviceId;
        }

        /// <inheritdoc/>
        public override IServiceCollection AddToServices(IServiceCollection services)
        {
            IChatClient chatClient = new OllamaApiClient(Uri, Model);
            services.AddKeyedChatClient(ServiceId, chatClient);
            return services;
        }
    }
}
