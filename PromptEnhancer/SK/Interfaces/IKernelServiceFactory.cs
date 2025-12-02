using PromptEnhancer.KernelServiceTemplates;
using PromptEnhancer.Models.Configurations;

namespace PromptEnhancer.SK.Interfaces
{
    public interface IKernelServiceFactory
    {
        public IEnumerable<IKernelServiceTemplate> CreateKernelServicesConfig(IEnumerable<KernelServiceBaseConfig> configs);
    }
}
