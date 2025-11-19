using ErrorOr;
using PromptEnhancer.Models.Pipeline;

namespace PromptEnhancer.Models
{
    public class ResultModel
    {
        public PipelineContext? Result { get; init; }
        public IEnumerable<Error> Errors { get; init; } = [];
        public bool PipelineSuccess => !Errors.Any();
    }
}
