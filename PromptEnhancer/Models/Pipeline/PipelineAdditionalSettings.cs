using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel;

namespace PromptEnhancer.Models.Pipeline
{
    public class PipelineAdditionalSettings
    {
        public ChatOptions ChatOptions { get; init; } = new ChatOptions() { MaxOutputTokens = 1000 };

        public PromptExecutionSettings? KernelRequestSettings { get; init; }

        public string? GeneratorKey { get; init; }
        public string? ChatClientKey { get; init; }
        public int MaximumInputLength { get; init; } = 10000;
    }
}
