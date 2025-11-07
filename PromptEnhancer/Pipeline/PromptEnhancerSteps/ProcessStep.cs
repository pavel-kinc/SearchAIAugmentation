using ErrorOr;
using PromptEnhancer.Models.Pipeline;

namespace PromptEnhancer.Pipeline.PromptEnhancerSteps
{
    public class ProcessStep : PipelineStep
    {
        public ProcessStep(bool isRequired = false)
        {
            _isRequired = isRequired;
        }

        protected override Task<ErrorOr<bool>> ExecuteStepAsync(PipelineSettings settings, PipelineContext context, CancellationToken cancellationToken = default)
        {
            if (context.QueryString is null)
            {
                return Task.FromResult<ErrorOr<bool>>(false);
            }

            context.QueryString = context.QueryString.Trim();
            return Task.FromResult<ErrorOr<bool>>(true);
        }

        protected override ErrorOr<bool> CheckExecuteConditions(PipelineContext context)
        {
            if (!string.IsNullOrEmpty(context.QueryString))
            {
                return true;
            }

            return FailCondition();
        }
    }
}
