using ErrorOr;
using PromptEnhancer.Models.Pipeline;

namespace PromptEnhancer.Pipeline.PromptEnhancerSteps
{
    public class PreprocessStep : PipelineStep
    {
        public PreprocessStep(bool isRequired = false) : base(isRequired) { }
        protected async override Task<ErrorOr<bool>> ExecuteStepAsync(PipelineSettings settings, PipelineRun context, CancellationToken cancellationToken = default)
        {
            if (context.QueryString is null)
            {
                return false;
            }

            context.QueryString = context.QueryString.Trim();
            return true;
        }

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
