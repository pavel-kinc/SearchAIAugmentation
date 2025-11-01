using ErrorOr;
using PromptEnhancer.Models.Pipeline;
using PromptEnhancer.Pipeline.Interfaces;

namespace PromptEnhancer.Pipeline
{
    public class PipelineOrchestrator
    {
        //TODO maybe add strategy for when step fails? also params - this is in the step itself now
        public virtual async Task<ErrorOr<bool>> RunPipeline(Queue<IPipelineStep> steps, PipelineContext context)
        {
            while (steps.Any())
            {
                var step = steps.Dequeue();
                var result = await step.ExecuteAsync(context);
                if (result.IsError)
                {
                    //TODO logging
                    return result;
                }
            }
            return true;
        }
    }
}
