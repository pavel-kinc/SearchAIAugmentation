using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel;
using PromptEnhancer.Models.Configurations;

namespace PromptEnhancer.Models.Pipeline
{
    public class PipelineAdditionalSettings
    {
        public ChatOptions? ChatOptions { get; init; }

        public PromptExecutionSettings? KernelRequestSettings { get; init; }

        public PromptConfiguration? PromptConfiguration { get; init; }

        public string? GeneratorKey { get; init; } = null;
        public string? ChatClientKey { get; init; } = null;
    }
}
