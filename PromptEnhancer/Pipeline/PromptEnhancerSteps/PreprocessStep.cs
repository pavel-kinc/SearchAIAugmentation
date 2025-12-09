using ErrorOr;
using PromptEnhancer.Models.Pipeline;

namespace PromptEnhancer.Pipeline.PromptEnhancerSteps
{
    /// <summary>
    /// Represents a preprocessing step in a pipeline that operates on the query string of the pipeline context.
    /// </summary>
    /// <remarks>This step trims leading and trailing whitespace from the query string in the pipeline
    /// context.  It also includes a mechanism to check whether the step can be executed based on the current state of
    /// the context.</remarks>
    public class PreprocessStep : PipelineStep
    {
        public PreprocessStep(bool isRequired = false) : base(isRequired) { }

        /// <inheritdoc/>
        protected async override Task<ErrorOr<bool>> ExecuteStepAsync(PipelineSettings settings, PipelineRun context, CancellationToken cancellationToken = default)
        {
            if (context.QueryString is null)
            {
                return false;
            }

            context.QueryString = context.QueryString.Trim();
            return true;
        }

        /// <inheritdoc/>
        protected override ErrorOr<bool> CheckExecutionConditions(PipelineRun context)
        {
            if (!string.IsNullOrEmpty(context.QueryString))
            {
                return true;
            }

            return FailCondition();
        }
    }
}
