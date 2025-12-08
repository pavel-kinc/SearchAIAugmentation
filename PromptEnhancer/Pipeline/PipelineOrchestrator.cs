using ErrorOr;
using PromptEnhancer.Models.Pipeline;
using PromptEnhancer.Pipeline.Interfaces;

namespace PromptEnhancer.Pipeline
{
    /// <inheritdoc/>
    public class PipelineOrchestrator : IPipelineOrchestrator
    {
        /// <inheritdoc/>
        public virtual async Task<ErrorOr<bool>> RunPipelineAsync(PipelineModel pipeline, PipelineRun context, CancellationToken ct = default)
        {
            var steps = pipeline.Steps;
            if (steps is null || !steps.Any())
            {
                //TODO logging
                return Error.Failure("Pipeline has no steps to execute.");
            }

            foreach (var step in steps)
            {
                try
                {
                    var result = await step.ExecuteAsync(pipeline.Settings, context, ct);
                    if (result.IsError)
                    {
                        context.PipelineLog.Add($"Error in step: {GetType().Name}, {string.Join(';', result.Errors.Select(x => x.Code))}");
                        return result;
                    }
                    if (result.Value)
                    {
                        context.PipelineLog.Add($"Step {GetType().Name} was successfully executed.");
                    }
                    else
                    {
                        context.PipelineLog.Add($"Step {GetType().Name} was not executed. Continuing in the pipeline.");
                    }

                }
                catch (Exception ex)
                {
                    context.PipelineLog.Add($"Exception in step: {GetType().Name}, {ex.Message}");
                    return Error.Failure($"Error: {step.GetType().Name} has thrown Exception in step execution", ex.Message);
                }
            }
            return true;
        }
    }
}
