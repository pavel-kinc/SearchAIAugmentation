using ErrorOr;
using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel;
using PromptEnhancer.AIUtility.ChatHistory;
using PromptEnhancer.Models.Pipeline;

namespace PromptEnhancer.Pipeline.PromptEnhancerSteps
{
    /// <summary>
    /// Represents a pipeline step that generates context for a user query by invoking context functions and processing
    /// their results based on the specified prompt template. 
    /// This step includes Kernel function calling and works only with automatic choice behavior and OpenAI.
    /// </summary>
    /// <remarks>This step is designed to interact with context functions to retrieve relevant information for
    /// a user query. The results are formatted as a single string, with each result separated by a specified separator
    /// character. If no relevant functions are found, a predefined constant value is returned.</remarks>
    public class KernelContextPluginsStep : PipelineStep
    {
        public const string LLMResponseNoResult = "NaN";
        public const int MaxContextItemSize = 100;
        public const char ResultSeperator = ';';

        /// <summary>
        /// Gets the template for generating a context prompt based on a user query and context functions.
        /// </summary>
        public virtual string ContextPromptTemplate =>
            """
            Get context for user query: "{0}" from context functions, where there is real connection between their result and the query.
            • Output a single string with each result separated by a separator '{1}'.
            • If no functions are considered to be relevant to the user query just output "{2}".
            Do not output anything else (no extra explanation, no formatting other than the string described).
            """;

        public KernelContextPluginsStep(bool isRequired = false) : base(isRequired) { }

        /// <inheritdoc/>
        /// <remarks>This method validates the kernel request settings and ensures the input prompt length
        /// does not exceed the maximum allowed length. If the preconditions are satisfied, it invokes the kernel with
        /// the generated prompt and updates the pipeline run context with token usage and additional context extracted
        /// from the kernel's response.</remarks>
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

            // invokes the prompt against the kernel, resulting in fucntion calling
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

        /// <inheritdoc/>
        protected override ErrorOr<bool> CheckExecutionConditions(PipelineRun context)
        {
            if (!string.IsNullOrEmpty(context.QueryString))
            {
                return true;
            }

            return FailCondition();
        }

        /// <summary>
        /// Generates a formatted prompt string based on the provided query string.
        /// </summary>
        /// <param name="queryString">The query string to include in the prompt. If <see langword="null"/>, a default response is returned.</param>
        /// <returns>A formatted string containing the query string and additional context, or a default response if <paramref
        /// name="queryString"/> is <see langword="null"/>.</returns>
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
