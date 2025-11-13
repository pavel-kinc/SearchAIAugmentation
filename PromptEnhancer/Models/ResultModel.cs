using ErrorOr;
using PromptEnhancer.Models.Pipeline;

namespace PromptEnhancer.Models
{
    public class ResultModel
    {
        public string? Query { get; set; }

        public string Prompt { get; set; } = string.Empty;

        public string SearchResult { get; set; } = string.Empty;

        public ChatCompletionResult? AIResult { get; set; }

        public PipelineContext? Result { get; init; }

        public ErrorOr<bool>? PipelineSuccess { get; init; }

        public Entry? EntryInput { get; init; }
    }
}
