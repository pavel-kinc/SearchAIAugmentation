using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;

namespace PromptEnhancer.Models.Pipeline
{
    public class PipelineSettings(
        Kernel kernel,
        IServiceProvider sp)
    {
        public Kernel Kernel { get; } = kernel;

        public IServiceProvider ServiceProvider { get; set; } = sp;

        public T GetService<T>(string? key = null)
            where T : class
        {
            if (key is null)
            {
                return sp.GetRequiredService<T>();
            }

            return sp.GetRequiredKeyedService<T>(key);


        }
    }

}
