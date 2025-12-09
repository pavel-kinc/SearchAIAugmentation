using Microsoft.Extensions.DependencyInjection;

namespace PromptEnhancer.KernelServiceTemplates
{
    /// <inheritdoc/>
    /// abstract base class for kernel service templates
    public abstract class KernelServiceTemplate : IKernelServiceTemplate
    {
        /// <inheritdoc/>
        public abstract IServiceCollection AddToServices(IServiceCollection services);

        /// <summary>
        /// Used for common setup logic in derived classes
        /// </summary>
        public virtual void SetupBaseServiceTemplate()
        {
            // Common setup logic for all derived classes can go here
        }
    }
}
