using ErrorOr;
using Microsoft.Extensions.AI;
using PromptEnhancer.AIUtility.ChatHistory;
using PromptEnhancer.AIUtility.TokenCountFallback;
using PromptEnhancer.Models.Pipeline;

namespace PromptEnhancer.Pipeline.PromptEnhancerSteps
{
    /// <summary>
    /// Represents a pipeline step responsible for generating a response based on the provided chat history, prompt, and
    /// settings.
    /// </summary>
    /// <remarks>This step interacts with an <see cref="IChatClient"/> to generate a response using the chat
    /// history and options  specified in the pipeline settings. It also updates the pipeline run context with token
    /// usage and the final response.</remarks>
    public class GenerationStep : PipelineStep
    {
        public GenerationStep(bool isRequired = false) : base(isRequired) { }

        /// <inheritdoc/>
        /// <remarks>This method interacts with a chat client to process the chat history and generate a
        /// response. It ensures that the input length does not exceed the configured maximum. If the input length
        /// exceeds the limit, the step fails with an appropriate error message. The method updates the pipeline run
        /// context with token usage statistics and the final response.</remarks>
        protected async override Task<ErrorOr<bool>> ExecuteStepAsync(PipelineSettings settings, PipelineRun context, CancellationToken cancellationToken = default)
        {
            var chatClient = settings.Kernel.GetRequiredService<IChatClient>(settings.Settings.ChatClientKey);
            List<ChatMessage> history = ChatHistoryUtility.AddToChatHistoryPipeline(context);
            if (ChatHistoryUtility.GetHistoryLength(history) > settings.Settings.MaximumInputLength)
            {
                return FailExecution(ChatHistoryUtility.GetInputSizeExceededLimitMessage(GetType().Name));
            }

            var res = await chatClient.GetResponseAsync(history, settings.Settings.ChatOptions, cancellationToken);
            context.InputTokenUsage += res.Usage?.InputTokenCount ?? TokenCounter.CountTokens(history);
            context.OutputTokenUsage += res.Usage?.OutputTokenCount ?? TokenCounter.CountTokens(res.Messages);
            context.FinalResponse = res;
            history.AddRange(res.Messages);
            context.ChatHistory = history;
            return true;
        }

        /// <inheritdoc/>
        protected override ErrorOr<bool> CheckExecutionConditions(PipelineRun context)
        {
            if (!string.IsNullOrEmpty(context.QueryString) || !string.IsNullOrEmpty(context.UserPromptToLLM))
            {
                return true;
            }

            return FailCondition();
        }
    }
}
