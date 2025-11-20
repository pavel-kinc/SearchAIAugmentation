using PromptEnhancer.Models.Enums;

namespace PromptEnhancer.Models.Configurations
{
    public class KernelServiceBaseConfig
    {
        public AIProviderEnum KernelServiceProvider { get; set; }
        public string? DeploymentName { get; set; }
        // ModelId, endpoint, onnxmodelpath
        public string Model { get; set; }
        // ApiKey, path, uri, vocabpath
        public string Key { get; set; }

        public string? ServiceId { get; set; }

        public KernelServiceEnum KernelServiceType { get; set; }

        public KernelServiceBaseConfig(AIProviderEnum kernelServiceProvider, string model, string key, string? deploymentName = null, string? serviceId = null, KernelServiceEnum serviceType = KernelServiceEnum.ChatClient)
        {
            KernelServiceProvider = kernelServiceProvider;
            Model = model;
            Key = key;
            DeploymentName = deploymentName;
            ServiceId = serviceId;
            KernelServiceType = serviceType;
        }
    }
}
