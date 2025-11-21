using ErrorOr;
using PromptEnhancer.Models.Pipeline;

namespace PromptEnhancer.Models
{
    public class ResultModel
    {
        public PipelineRun? Result { get; init; }
        public IEnumerable<Error> Errors { get; init; } = [];
        public bool PipelineSuccess => !Errors.Any();
    }
}
