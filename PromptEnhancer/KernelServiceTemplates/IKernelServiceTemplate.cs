using Microsoft.Extensions.DependencyInjection;

namespace PromptEnhancer.KernelServiceTemplates
{
    public interface IKernelServiceTemplate
    {
        public IServiceCollection AddToServices(IServiceCollection services);
    }
}
