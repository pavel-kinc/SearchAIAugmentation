using PromptEnhancer.KernelServiceTemplates;
using PromptEnhancer.Models.Configurations;

namespace PromptEnhancer.Extensions
{
    public interface IKernelServiceFactory
    {
        public IEnumerable<IKernelServiceTemplate> CreateKernelServicesConfig(IEnumerable<KernelServiceBaseConfig> configs);
    }
}
