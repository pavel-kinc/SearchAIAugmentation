using ErrorOr;
using PromptEnhancer.Models.Pipeline;
using PromptEnhancer.Pipeline.Interfaces;

namespace PromptEnhancer.Pipeline
{
    public class PipelineOrchestrator : IPipelineOrchestrator
    {
        public virtual async Task<ErrorOr<bool>> RunPipelineAsync(PipelineModel pipeline, PipelineContext context, CancellationToken ct = default)
        {
            var steps = pipeline.Steps;
            if (steps is null || !steps.Any())
            {
                //TODO logging
                return Error.Failure("Pipeline has no steps to execute.");
            }

            foreach (var step in steps)
            {
                //true means step executed correctly, false means step did not execute/start (for unrequired steps), otherwise error
                try
                {
                    var result = await step.ExecuteAsync(pipeline.Settings, context, ct);
                    if (result.IsError)
                    {
                        //TODO logging
                        return result;
                    }
                }
                catch(Exception ex)
                {
                    return Error.Failure($"Error: {nameof(step)} has thrown Exception in step execution", ex.Message);
                }
            }
            return true;
        }
    }
}
