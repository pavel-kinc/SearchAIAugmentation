using ErrorOr;
using PromptEnhancer.Models.Pipeline;

namespace PromptEnhancer.Pipeline.Interfaces
{
    public interface IPipelineOrchestrator
    {
        public Task<ErrorOr<bool>> RunPipelineAsync(PipelineModel pipeline, PipelineRun context, CancellationToken ct = default);
    }
}
