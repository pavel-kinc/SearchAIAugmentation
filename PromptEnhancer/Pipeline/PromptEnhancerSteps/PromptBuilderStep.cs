using ErrorOr;
using PromptEnhancer.Models.Pipeline;
using PromptEnhancer.Services.PromptBuildingService;

namespace PromptEnhancer.Pipeline.PromptEnhancerSteps
{
    //TODO FINISH
    public class PromptBuilderStep : PipelineStep
    {
        private readonly string? _promptBuildingServiceKey;

        public PromptBuilderStep(string? promptBuildingServiceKey = null, bool isRequired = false) : base(isRequired)
        {
            _promptBuildingServiceKey = promptBuildingServiceKey;
        }
        protected async override Task<ErrorOr<bool>> ExecuteStepAsync(PipelineSettings settings, PipelineContext context, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(context.QueryString))
            {
                return false;
            }

            var promptBuildingService = settings.GetService<IPromptBuildingService>(_promptBuildingServiceKey);
            context.SystemPromptToLLM ??= promptBuildingService.BuildSystemPrompt(settings.PromptConfiguration);
            context.UserPromptToLLM = promptBuildingService.BuildUserPrompt(context.QueryString, context.PickedRecords, context.AdditionalContext, context.Entry);

            return true;
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
