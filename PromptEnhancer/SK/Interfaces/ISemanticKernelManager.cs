using ErrorOr;
using Microsoft.SemanticKernel;
using PromptEnhancer.Models.Configurations;

namespace PromptEnhancer.SK.Interfaces
{
    /// <summary>
    /// Defines methods for managing and interacting with a Semantic Kernel, including adding plugins, creating
    /// kernels, and converting kernel configurations.
    /// </summary>
    /// <remarks>This interface provides functionality to extend and configure Semantic Kernel instances. It
    /// includes methods for adding plugins, creating kernels with specific configurations, and converting kernel
    /// configuration data into a usable format.</remarks>
    public interface ISemanticKernelManager
    {
        /// <summary>
        /// Adds a plugin of the specified type to the provided Semantic Kernel instance.
        /// </summary>
        /// <remarks>The plugin type must be a class. Ensure that the <paramref name="kernel"/> instance
        /// is properly initialized  before calling this method.</remarks>
        /// <typeparam name="Plugin">The type of the plugin to add. Must be a class.</typeparam>
        /// <param name="kernel">The Semantic Kernel instance to which the plugin will be added.</param>
        public void AddPluginToSemanticKernel<Plugin>(Kernel kernel) where Plugin : class;

        /// <summary>
        /// Creates a new instance of a <see cref="Kernel"/> configured with the specified services and optional
        /// internal components.
        /// </summary>
        /// <remarks>The method allows for flexible configuration of the kernel by specifying a collection
        /// of service configurations.  Internal services and context plugins can be optionally included based on the
        /// provided parameters.</remarks>
        /// <param name="kernelServiceConfigs">A collection of service configurations used to initialize the kernel. Each configuration defines a service
        /// to be added to the kernel.</param>
        /// <param name="addInternalServices">A boolean value indicating whether to include internal services in the kernel.  <see langword="true"/> to
        /// add internal services; otherwise, <see langword="false"/>. The default is <see langword="false"/>.</param>
        /// <param name="addContextPlugins">A boolean value indicating whether to include context plugins in the kernel.  <see langword="true"/> to add
        /// context plugins; otherwise, <see langword="false"/>. The default is <see langword="true"/>.</param>
        /// <returns>An <see cref="ErrorOr{T}"/> containing the created <see cref="Kernel"/> instance if successful, or an error
        /// if the operation fails.</returns>
        public ErrorOr<Kernel> CreateKernel(IEnumerable<KernelServiceBaseConfig> kernelServiceConfigs, bool addInternalServices = false, bool addContextPlugins = true);

        /// <summary>
        /// Converts the provided kernel configuration data into a collection of kernel service base configurations (max 2 - chat client and embedding generator).
        /// </summary>
        /// <param name="kernelData">The kernel configuration data to be converted. This parameter cannot be null.</param>
        /// <returns>A collection of <see cref="KernelServiceBaseConfig"/> objects representing the converted configuration data.
        /// If the conversion fails, an <see cref="ErrorOr{T}"/> containing the error details is returned.</returns>
        public ErrorOr<IEnumerable<KernelServiceBaseConfig>> ConvertConfig(KernelConfiguration kernelData);
    }
}
