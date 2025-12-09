using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Onnx;

namespace PromptEnhancer.KernelServiceTemplates.ChatClients
{
    /// <summary>
    /// Service template for adding an Onnx Chat Client to the kernel's service collection.
    /// </summary>
    public class OnnxChatClient : KernelServiceTemplate
    {
        public string ModelId { get; }
        public string ModelPath { get; }
        public string? ServiceId { get; }

        public OnnxChatClient(string modelId, string modelPath, string? serviceId = null)
        {
            ModelId = modelId;
            ModelPath = modelPath;
            ServiceId = serviceId;
        }

        /// <inheritdoc/>
        public override IServiceCollection AddToServices(IServiceCollection services)
        {
            var chatClient = new OnnxRuntimeGenAIChatCompletionService(ModelId, ModelPath).AsChatClient();
            services.AddKeyedChatClient(ServiceId, chatClient);
            return services;
        }
    }
}
