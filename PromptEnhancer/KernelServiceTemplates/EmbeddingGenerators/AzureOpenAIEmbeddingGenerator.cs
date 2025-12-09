using Microsoft.Extensions.DependencyInjection;

namespace PromptEnhancer.KernelServiceTemplates.EmbeddingGenerators
{
    /// <summary>
    /// Service template for adding an Azure OpenAI Embedding Generator to the kernel's service collection.
    /// </summary>
    public class AzureOpenAIEmbeddingGenerator : KernelServiceTemplate
    {
        public string DeploymentName { get; }
        public string Endpoint { get; }
        public string ApiKey { get; }
        public string? ServiceId { get; }

        public AzureOpenAIEmbeddingGenerator(string deploymentName, string endpoint, string apiKey, string? serviceId = null)
        {
            DeploymentName = deploymentName;
            Endpoint = endpoint;
            ApiKey = apiKey;
            ServiceId = serviceId;
        }

        /// <inheritdoc/>
        public override IServiceCollection AddToServices(IServiceCollection services)
        {
            return services.AddAzureOpenAIEmbeddingGenerator(DeploymentName, Endpoint, ApiKey, serviceId: ServiceId);
        }
    }
}
