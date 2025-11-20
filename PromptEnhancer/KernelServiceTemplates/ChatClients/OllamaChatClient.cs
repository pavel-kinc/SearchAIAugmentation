using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using OllamaSharp;

namespace PromptEnhancer.KernelServiceTemplates.ChatClients
{
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

        public override IServiceCollection AddToServices(IServiceCollection services)
        {
            IChatClient chatClient = new OllamaApiClient(Uri, Model);
            services.AddKeyedChatClient(ServiceId, chatClient);
            return services;
        }
    }
}
