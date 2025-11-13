using ErrorOr;
using PromptEnhancer.Models.Pipeline;

namespace PromptEnhancer.Pipeline.PromptEnhancerSteps
{
    //TODO FINISH
    public class PostProcessStep : PipelineStep
    {
        public PostProcessStep(bool isRequired = false) : base(isRequired) { }
        protected async override Task<ErrorOr<bool>> ExecuteStepAsync(PipelineSettings settings, PipelineContext context, CancellationToken cancellationToken = default)
        {
            return CheckContextState(context);
        }

        protected virtual ErrorOr<bool> CheckContextState(PipelineContext context)
        {
            if (context.PickedRecords.Any() && context.RetrievedRecords.Any())
            {
                return true;
            }

            return FailCondition();
        }
    }
}
