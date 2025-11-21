using ErrorOr;
using PromptEnhancer.Models.Pipeline;

namespace PromptEnhancer.Pipeline.PromptEnhancerSteps
{
    //TODO FINISH
    public class PostProcessCheckStep : PipelineStep
    {
        public PostProcessCheckStep(bool isRequired = false) : base(isRequired) { }
        protected async override Task<ErrorOr<bool>> ExecuteStepAsync(PipelineSettings settings, PipelineRun context, CancellationToken cancellationToken = default)
        {
            return CheckContextState(context);
        }

        protected virtual ErrorOr<bool> CheckContextState(PipelineRun context)
        {
            if (context.PickedRecords.Any() && context.RetrievedRecords.Any())
            {
                return true;
            }

            return FailCondition();
        }
    }
}
