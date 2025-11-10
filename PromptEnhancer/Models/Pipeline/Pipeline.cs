using PromptEnhancer.Pipeline.Interfaces;

namespace PromptEnhancer.Models.Pipeline
{
    public class Pipeline(
        PipelineSettings settings,
        IEnumerable<IPipelineStep> steps)
    {
        public PipelineSettings Settings { get; } = settings;

        public IReadOnlyList<IPipelineStep> Steps { get; } = [.. steps];
    }
}
