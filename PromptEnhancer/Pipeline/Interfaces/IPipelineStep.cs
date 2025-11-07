using ErrorOr;
using PromptEnhancer.Models.Pipeline;

namespace PromptEnhancer.Pipeline.Interfaces
{
    public interface IPipelineStep
    {
        public Task<ErrorOr<bool>> ExecuteAsync(PipelineSettings settings, PipelineContext context, bool isRequired = false, CancellationToken cancellationToken = default);
    }
}
