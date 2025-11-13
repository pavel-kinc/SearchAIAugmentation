using ErrorOr;
using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel;
using PromptEnhancer.Models.Pipeline;

namespace PromptEnhancer.Pipeline.PromptEnhancerSteps
{
    //TODO FINISH
    public class KernelContextPluginsStep : PipelineStep
    {
        public const string ContextPromptTemplate =
            """
            Get context for user query: "{0}" from context functions, where there is real connection between their result and the query.
            • Output a single string with each result separated by a separator '{1}'.
            • If no functions are considered to be relevant to the user query just output "{2}".
            Do not output anything else (no extra explanation, no formatting other than the string described).
            """;
        public const string LLMResponseNoResult = "NaN";
        public const int MaxContextItemSize = 100;
        public const char ResultSeperator = ';';

        public KernelContextPluginsStep(bool isRequired = false) : base(isRequired) { }
        protected async override Task<ErrorOr<bool>> ExecuteStepAsync(PipelineSettings settings, PipelineContext context, CancellationToken cancellationToken = default)
        {
            if (settings.KernelRequestSettings is null || settings.KernelRequestSettings.FunctionChoiceBehavior != FunctionChoiceBehavior.Auto())
            {
                return FailCondition();
            }
            var res = await settings.Kernel.InvokePromptAsync<ChatResponse>(GetPrompt(context.QueryString), new(settings.KernelRequestSettings), cancellationToken: cancellationToken);
            var tokenUsage = res?.Usage?.TotalTokenCount;
            var stringResult = res?.Text;
            if (stringResult != null && stringResult != LLMResponseNoResult)
            {
                context.AdditionalContext.AddRange(stringResult.Split(ResultSeperator).Where(x => x.Length <= MaxContextItemSize));
            }
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
        private string GetPrompt(string? queryString)
        {
            if (queryString is null)
            {
                return LLMResponseNoResult;
            }
            return string.Format(ContextPromptTemplate, queryString, ResultSeperator, LLMResponseNoResult);
        }
    }
}
