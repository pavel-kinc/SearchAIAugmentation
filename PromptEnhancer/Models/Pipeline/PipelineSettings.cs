using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using PromptEnhancer.Models.Configurations;

namespace PromptEnhancer.Models.Pipeline
{
    public class PipelineSettings(
    Kernel kernel,
    IServiceProvider sp)
    {
        public Kernel Kernel { get; } = kernel;

        public IServiceProvider ServiceProvider { get; } = sp;

        public ChatOptions? ChatOptions { get; set; }

        public PromptConfiguration? PromptConfiguration { get; init; }

        public T GetService<T>(string? key = null)
            where T : class
        {
            if (key is null)
            {
                return ServiceProvider.GetRequiredService<T>();
            }

            return ServiceProvider.GetRequiredKeyedService<T>(key);
        }

        public string? GeneratorKey { get; init; } = null;
        public string? ChatClientKey { get; init; } = null;
    }

}
