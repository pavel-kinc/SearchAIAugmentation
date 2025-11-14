using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;

namespace PromptEnhancer.Models.Pipeline
{
    public class PipelineSettings(
    Kernel kernel,
    IServiceProvider sp,
    PipelineAdditionalSettings settings)
    {
        public Kernel Kernel { get; } = kernel;

        public IServiceProvider ServiceProvider { get; } = sp;

        public PipelineAdditionalSettings Settings { get; } = settings;

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
