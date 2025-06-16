using DemoApp.Models;
using PromptEnhancer.Models.Configurations;

namespace DemoApp.Services
{
    public interface IConfigurationSetupService
    {
        ConfigurationSetup GetConfiguration();
        void UpdateKernelConfig(KernelConfiguration kernelConfiguration);
        void UpdateSearchConfig(SearchConfiguration searchConfiguration);
    }
}
