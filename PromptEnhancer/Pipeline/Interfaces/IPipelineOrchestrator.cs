using ErrorOr;
using PromptEnhancer.Models.Pipeline;

namespace PromptEnhancer.Pipeline.Interfaces
{
    public interface IPipelineOrchestrator
    {
        public Task<ErrorOr<bool>> RunPipelineAsync(Models.Pipeline.Pipeline pipeline, PipelineContext context);
    }
}
