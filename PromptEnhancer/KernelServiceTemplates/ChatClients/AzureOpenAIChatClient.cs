using Microsoft.Extensions.DependencyInjection;

namespace PromptEnhancer.KernelServiceTemplates.ChatClients
{
    public class AzureOpenAIChatClient : KernelServiceTemplate
    {
        public string DeploymentName { get; }
        public string Endpoint { get; }
        public string ApiKey { get; }
        public string? ServiceId { get; }

        public AzureOpenAIChatClient(string deploymentName, string endpoint, string apiKey, string? serviceId = null)
        {
            DeploymentName = deploymentName;
            Endpoint = endpoint;
            ApiKey = apiKey;
            ServiceId = serviceId;
        }

        public override IServiceCollection AddToServices(IServiceCollection services)
        {
            return services.AddAzureOpenAIChatClient(DeploymentName, Endpoint, ApiKey, serviceId: ServiceId);
        }
    }
}
