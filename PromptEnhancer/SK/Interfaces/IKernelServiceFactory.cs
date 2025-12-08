using PromptEnhancer.KernelServiceTemplates;
using PromptEnhancer.Models.Configurations;

namespace PromptEnhancer.SK.Interfaces
{
    /// <summary>
    /// Defines a factory for creating kernel service configurations based on the provided base configurations.
    /// </summary>
    /// <remarks>This interface is intended to be implemented by classes that generate kernel service
    /// configurations from a collection of base configuration objects. The resulting configurations are used to
    /// initialize or configure kernel services.</remarks>
    public interface IKernelServiceFactory
    {
        /// <summary>
        /// Creates a collection of kernel service templates based on the provided configurations.
        /// </summary>
        /// <param name="configs">A collection of <see cref="KernelServiceBaseConfig"/> objects that define the configurations  for creating
        /// kernel service templates. Cannot be null.</param>
        /// <returns>An enumerable collection of <see cref="IKernelServiceTemplate"/> instances created from the specified
        /// configurations.</returns>
        public IEnumerable<IKernelServiceTemplate> CreateKernelServicesConfig(IEnumerable<KernelServiceBaseConfig> configs);
    }
}
