using PromptEnhancer.Models.Pipeline;

namespace PromptEnhancer.Pipeline.Interfaces
{
    public interface IPipelineContextService
    {
        PipelineContext? GetCurrentContext();

        public void SetCurrentContext(PipelineContext context);
    }
}
