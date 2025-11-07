using PromptEnhancer.Models.Pipeline;
using PromptEnhancer.Pipeline.Interfaces;

namespace PromptEnhancer.Models.Configurations
{
    public class EnhancerConfiguration
    {
        public KernelConfiguration? KernelConfiguration { get; set; }

        public SearchConfiguration SearchConfiguration { get; set; } = new();
        public PromptConfiguration PromptConfiguration { get; set; } = new();
        public IEnumerable<IPipelineStep>? PipeLineSteps { get; set; }
        public bool UseAutomaticFunctionCalling { get; set; } = false;
    }
}
