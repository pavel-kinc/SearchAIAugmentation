using ErrorOr;
using PromptEnhancer.Models.Pipeline;
using PromptEnhancer.Pipeline.Interfaces;

namespace PromptEnhancer.Pipeline
{
    public class PipelineOrchestrator : IPipelineOrchestrator
    {
        //TODO maybe add strategy for when step fails? also params - this is in the step itself now
        //TODO Pipeline for every query or reusable - also class for only pipeline?
        public virtual async Task<ErrorOr<bool>> RunPipelineAsync(Models.Pipeline.PipelineModel pipeline, PipelineContext context)
        {
            var steps = pipeline.Steps;
            if (steps is null || !steps.Any())
            {
                //TODO logging
                return Error.Failure("Pipeline has no steps to execute.");
            }

            foreach (var step in steps)
            {
                var result = await step.ExecuteAsync(pipeline.Settings, context);
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
