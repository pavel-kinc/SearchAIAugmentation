using Microsoft.Extensions.DependencyInjection;

namespace PromptEnhancer.KernelServiceTemplates
{
    public abstract class KernelServiceTemplate : IKernelServiceTemplate
    {
        public abstract IServiceCollection AddToServices(IServiceCollection services);

        public virtual void SetupBaseServiceTemplate()
        {
            // Common setup logic for all derived classes can go here
        }
    }
}
