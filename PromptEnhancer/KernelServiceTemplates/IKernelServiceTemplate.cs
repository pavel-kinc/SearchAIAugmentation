using Microsoft.Extensions.DependencyInjection;

namespace PromptEnhancer.KernelServiceTemplates
{
    /// <summary>
    /// Defines a contract for a kernel/feature service “template” that centralizes registration of its services
    /// into a dependency injection container.
    /// </summary>
    public interface IKernelServiceTemplate
    {
        /// <summary>
        /// Adds and configures this kernel/template's services into the provided dependency injection container.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to which services will be added. Must not be null.</param>
        /// <returns>The same <see cref="IServiceCollection"/> instance, enabling method chaining.</returns>
        public IServiceCollection AddToServices(IServiceCollection services);
    }
}
