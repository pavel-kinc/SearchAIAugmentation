using ErrorOr;
using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel;
using PromptEnhancer.AIUtility.ChatHistory;
using PromptEnhancer.Models.Pipeline;

namespace PromptEnhancer.Pipeline.PromptEnhancerSteps
{
    public class KernelContextPluginsStep : PipelineStep
    {
        public const string LLMResponseNoResult = "NaN";
        public const int MaxContextItemSize = 100;
        public const char ResultSeperator = ';';

        public virtual string ContextPromptTemplate =>
            """
            Get context for user query: "{0}" from context functions, where there is real connection between their result and the query.
            • Output a single string with each result separated by a separator '{1}'.
            • If no functions are considered to be relevant to the user query just output "{2}".
            Do not output anything else (no extra explanation, no formatting other than the string described).
            """;

        public KernelContextPluginsStep(bool isRequired = false) : base(isRequired) { }
        protected async override Task<ErrorOr<bool>> ExecuteStepAsync(PipelineSettings settings, PipelineRun context, CancellationToken cancellationToken = default)
        {
            if (settings.Settings.KernelRequestSettings is null || settings.Settings.KernelRequestSettings.FunctionChoiceBehavior?.GetType() != FunctionChoiceBehavior.Auto().GetType())
            {
                return false;
            }
            var prompt = GetPrompt(context.QueryString);
            if (prompt.Length > settings.Settings.MaximumInputLength)
            {
                return FailExecution(ChatHistoryUtility.GetInputSizeExceededLimitMessage(GetType().Name));
            }

            var res = await settings.Kernel.InvokePromptAsync<ChatResponse>(prompt, new(settings.Settings.KernelRequestSettings), cancellationToken: cancellationToken);
            var tokenUsage = res?.Usage?.TotalTokenCount;
            context.InputTokenUsage += res?.Usage?.InputTokenCount ?? 0;
            context.OutputTokenUsage += res?.Usage?.OutputTokenCount ?? 0;
            var stringResult = res?.Text;
            if (stringResult != null && stringResult != LLMResponseNoResult)
            {
                context.AdditionalContext.AddRange(stringResult.Split(ResultSeperator).Where(x => x.Length <= MaxContextItemSize && x != LLMResponseNoResult && !string.IsNullOrEmpty(x)));
            }
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
        protected string GetPrompt(string? queryString)
        {
            if (queryString is null)
            {
                return LLMResponseNoResult;
            }
            return string.Format(ContextPromptTemplate, queryString, ResultSeperator, LLMResponseNoResult);
        }
    }
}
