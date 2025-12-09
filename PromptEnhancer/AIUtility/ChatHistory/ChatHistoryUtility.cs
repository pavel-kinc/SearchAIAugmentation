using ErrorOr;
using Microsoft.Extensions.AI;
using PromptEnhancer.Models.Configurations;
using PromptEnhancer.Models.Pipeline;

namespace PromptEnhancer.AIUtility.ChatHistory
{
    /// <summary>
    /// Provides utility methods for managing and interacting with chat history.
    /// </summary>
    /// <remarks>This static class includes methods to add messages to a chat history, calculate the total
    /// length of chat messages, and generate error messages related to input size limits. It is designed to be used in
    /// the context of a pipeline run, where chat history management is required.</remarks>
    public static class ChatHistoryUtility
    {
        /// <summary>
        /// Adds the current user prompt and, if necessary, the system prompt to the chat history.
        /// </summary>
        /// <remarks>If the chat history is empty and a system prompt is provided, the system prompt is
        /// added to the history before the user prompt. This ensures that the system prompt is included in the
        /// conversation context.</remarks>
        /// <param name="context">The pipeline run context containing the chat history and prompts.</param>
        /// <returns>A list of <see cref="ChatMessage"/> objects representing the updated chat history.</returns>
        public static List<ChatMessage> AddToChatHistoryPipeline(PipelineRun context)
        {
            var history = context.ChatHistory?.ToList() ?? [];

            // if empty, add system role from prompt config
            if (context.SystemPromptToLLM is not null && !history.Any(x => x.Role == ChatRole.System))
            {
                history.Add(new ChatMessage(ChatRole.System, context.SystemPromptToLLM));
            }
            history.Add(new ChatMessage(ChatRole.User, context.UserPromptToLLM));
            return history;
        }

        /// <summary>
        /// Calculates the total length of all text messages in the chat history.
        /// </summary>
        /// <param name="history">The collection of chat messages to evaluate. Cannot be null.</param>
        /// <returns>The sum of the lengths of all text messages in the provided chat history.</returns>
        public static int GetHistoryLength(IEnumerable<ChatMessage> history)
        {
            return history.Sum(x => x.Text.Length);
        }

        /// <summary>
        /// Generates a message indicating that the input size limit has been exceeded.
        /// </summary>
        /// <param name="source">The source identifier to include in the message, typically representing the context or component where the
        /// limit was exceeded.</param>
        /// <returns>A formatted string message indicating the input size limit has been exceeded, including the specified
        /// source.</returns>
        public static string GetInputSizeExceededLimitMessage(string source)
        {
            return $"{source}: chat history exceeded input size limit. You can modify it in {nameof(PipelineAdditionalSettings)} for {nameof(EnhancerConfiguration)}.";
        }

        /// <summary>
        /// Creates an error indicating that the input size has exceeded the allowed limit.
        /// </summary>
        /// <param name="source">The source identifier to include in the message, typically representing the context or component where the
        /// limit was exceeded.</param>
        /// <returns>An <see cref="Error"/> object representing the failure due to input size exceeding the limit.</returns>
        public static Error GetInputSizeExceededLimitError(string source)
        {
            return Error.Failure(GetInputSizeExceededLimitMessage(source));
        }
    }
}
