using ErrorOr;
using PromptEnhancer.Models.Pipeline;

namespace PromptEnhancer.Models
{
    public class ResultModel
    {
        public PipelineContext? Result { get; init; }

        public ErrorOr<bool>? PipelineSuccess { get; init; }

        public Entry? EntryInput { get; init; }
    }
}
