using ErrorOr;
using PromptEnhancer.Models.Pipeline;
using PromptEnhancer.Services.PromptBuildingService;

namespace PromptEnhancer.Pipeline.PromptEnhancerSteps
{
    /// <summary>
    /// Represents a pipeline step responsible for building prompts for a language model (LLM) based on the provided
    /// query string and context.
    /// </summary>
    public class PromptBuilderStep : PipelineStep
    {
        public PromptBuilderStep(bool isRequired = false) : base(isRequired)
        {
        }

        /// <inheritdoc/>
        /// <remarks>This method uses the <see cref="IPromptBuildingService"/> to generate prompts for the
        /// system and user based on the provided pipeline settings and context. The system prompt is initialized if it
        /// is not already set in the context.</remarks>
        protected async override Task<ErrorOr<bool>> ExecuteStepAsync(PipelineSettings settings, PipelineRun context, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(context.QueryString))
            {
                return false;
            }

            var promptBuildingService = settings.GetService<IPromptBuildingService>();
            context.SystemPromptToLLM ??= promptBuildingService.BuildSystemPrompt(settings.PromptConfiguration);
            context.UserPromptToLLM = promptBuildingService.BuildUserPrompt(context.QueryString, context.PickedRecords, context.AdditionalContext, context.Entry);

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
