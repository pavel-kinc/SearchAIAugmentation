using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using PromptEnhancer.Models.Configurations;

namespace PromptEnhancer.Models.Pipeline
{
    public class PipelineSettings
    {
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
