using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel;

namespace PromptEnhancer.Models.Pipeline
{
    /// <summary>
    /// Represents additional configuration settings for a pipeline, including chat options, kernel request settings,
    /// and input constraints.
    /// </summary>
    /// <remarks>This class provides optional settings that can be used to customize the behavior of a
    /// pipeline.  It includes settings for chat generation, kernel request execution, and input length
    /// limits.</remarks>
    public class PipelineAdditionalSettings
    {
        public ChatOptions ChatOptions { get; init; } = new ChatOptions() { MaxOutputTokens = 1000 };

        public PromptExecutionSettings? KernelRequestSettings { get; init; }

        public string? GeneratorKey { get; init; }
        public string? ChatClientKey { get; init; }
        public int MaximumInputLength { get; init; } = 10000;
    }
}
