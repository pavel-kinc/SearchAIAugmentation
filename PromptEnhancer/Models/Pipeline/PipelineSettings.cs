using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using PromptEnhancer.Models.Configurations;

namespace PromptEnhancer.Models.Pipeline
{
    /// <summary>
    /// Represents the configuration and dependencies required to execute a processing pipeline.
    /// </summary>
    /// <remarks>This class encapsulates the core components and settings needed for pipeline execution, 
    /// including the kernel, service provider, additional settings, and prompt configuration.  It also provides utility
    /// methods for retrieving services from the configured service provider.</remarks>
    public class PipelineSettings
    {
        /// <summary>
        /// Gets the kernel instance associated with the current context.
        /// </summary>
        public Kernel Kernel { get; }

        public IServiceProvider ServiceProvider { get; }

        public PipelineAdditionalSettings Settings { get; }

        public PromptConfiguration PromptConfiguration { get; }

        public PipelineSettings(
        Kernel kernel,
        IServiceProvider sp,
        PipelineAdditionalSettings settings,
        PromptConfiguration promptConfiguration)
        {
            Kernel = kernel;
            ServiceProvider = sp;
            Settings = settings;
            PromptConfiguration = promptConfiguration;
        }

        /// <summary>
        /// Retrieves a service of the specified type from the service provider.
        /// </summary>
        /// <remarks>If a key is provided, the method attempts to retrieve a keyed service of the
        /// specified type. Otherwise, it retrieves the default service of the specified type.</remarks>
        /// <typeparam name="T">The type of service to retrieve. Must be a reference type.</typeparam>
        /// <param name="key">An optional key used to identify the service. If <see langword="null"/>, the default service of type
        /// <typeparamref name="T"/> is retrieved.</param>
        /// <returns>An instance of the requested service of type <typeparamref name="T"/>.</returns>
        public T GetService<T>(string? key = null)
            where T : class
        {
            if (key is null)
            {
                return ServiceProvider.GetRequiredService<T>();
            }

            return ServiceProvider.GetRequiredKeyedService<T>(key);
        }
    }

}
