using Microsoft.SemanticKernel;
using PromptEnhancer.Pipeline.Interfaces;

namespace PromptEnhancer.Models.Pipeline
{
    public class PipelineSettings(
        Kernel kernel,
        IServiceProvider sp)
    {
        public Kernel Kernel { get; } = kernel;

        public IServiceProvider ServiceProvider { get; set; } = sp;
    }
}
