using ErrorOr;
using PromptEnhancer.Models.Pipeline;
using PromptEnhancer.Prompt;

namespace PromptEnhancer.Pipeline.PromptEnhancerSteps
{
    //TODO FINISH
    public class PromptBuilderStep : PipelineStep
    {
        protected async override Task<ErrorOr<bool>> ExecuteStepAsync(PipelineSettings settings, PipelineContext context, CancellationToken cancellationToken = default)
        {
            context.PromptToLLM = PromptUtility.BuildPrompt(settings.PromptConfiguration, context.Entry?.QueryString ?? context.QueryString, context.PickedRecords);
            return true;
        }

        protected override ErrorOr<bool> CheckExecuteConditions(PipelineContext context)
        {
            if (string.IsNullOrEmpty(context.PromptToLLM))
            {
                return true;
            }

            return FailCondition();
        }
    }
}
