using ErrorOr;
using Microsoft.Extensions.Logging;
using PromptEnhancer.Models.Pipeline;
using PromptEnhancer.Pipeline.Interfaces;

namespace PromptEnhancer.Pipeline
{
    /// <inheritdoc/>
    public class PipelineOrchestrator : IPipelineOrchestrator
    {
        private readonly ILogger<PipelineOrchestrator> _logger;

        public PipelineOrchestrator(ILogger<PipelineOrchestrator> logger)
        {
            _logger = logger;
        }
        /// <inheritdoc/>
        public virtual async Task<ErrorOr<bool>> RunPipelineAsync(PipelineModel pipeline, PipelineRun context, CancellationToken ct = default)
        {
            var steps = pipeline.Steps;
            if (steps is null || !steps.Any())
            {
                _logger.LogWarning("Pipeline has no steps to execute.");
                return Error.Failure("Pipeline has no steps to execute.");
            }

            foreach (var step in steps)
            {
                try
                {
                    var result = await step.ExecuteAsync(pipeline.Settings, context, ct);
                    if (result.IsError)
                    {
                        context.PipelineLog.Add($"Error in step: {step.GetType().Name}, {string.Join(';', result.Errors.Select(x => x.Code))}");
                        return result;
                    }
                    if (result.Value)
                    {
                        context.PipelineLog.Add($"Step {step.GetType().Name} was successfully executed.");
                    }
                    else
                    {
                        context.PipelineLog.Add($"Step {step.GetType().Name} was not executed. Continuing in the pipeline.");
                    }

                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Exception occurred while executing step {StepName}", step.GetType().Name);
                    context.PipelineLog.Add($"Exception in step: {step.GetType().Name}, {ex.Message}");
                    return Error.Failure($"Error: {step.GetType().Name} has thrown Exception in step execution", ex.Message);
                }
            }
            return true;
        }
    }
}
