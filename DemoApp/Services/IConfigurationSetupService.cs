using DemoApp.Models;

using PromptEnhancer.Models.Configurations;

namespace DemoApp.Services
{
    public interface IConfigurationSetupService
    {
        ConfigurationSetup GetConfiguration(bool withSecrets = false);
        void UpdateKernelConfig(KernelConfiguration kernelConfiguration);
        void UpdateSearchConfig(SearchConfiguration searchConfiguration);
        void UpdatePromptConfig(PromptConfiguration promptConfiguration);
        void UploadConfiguration(ConfigurationSetup configuration);
    }
}
