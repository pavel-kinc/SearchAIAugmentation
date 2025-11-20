using PromptEnhancer.Models.Pipeline;
using PromptEnhancer.Pipeline.Interfaces;

namespace PromptEnhancer.Models.Configurations
{
    public class EnhancerConfiguration
    {
        public KernelConfiguration? KernelConfiguration { get; set; }
        public PromptConfiguration PromptConfiguration { get; set; } = new();
        public PipelineAdditionalSettings PipelineAdditionalSettings { get; set; } = new();
        public IEnumerable<IPipelineStep> Steps { get; set; } = [];
    }
}
