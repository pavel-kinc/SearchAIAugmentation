using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Google;

namespace PromptEnhancer.KernelServiceTemplates.ChatClients
{
    public class GeminiChatClient : KernelServiceTemplate
    {
        public string ModelId { get; }
        public string ApiKey { get; }
        public string? ServiceId { get; }

        public GeminiChatClient(string modelId, string apiKey, string? serviceId = null)
        {
            ModelId = modelId;
            ApiKey = apiKey;
            ServiceId = serviceId;
        }

        public override IServiceCollection AddToServices(IServiceCollection services)
        {
            var chatClient = new GoogleAIGeminiChatCompletionService(ModelId, ApiKey).AsChatClient();
            services.AddKeyedChatClient(ServiceId, chatClient);
            return services;
        }
    }
}
