using ErrorOr;
using Microsoft.Extensions.AI;
using PromptEnhancer.Models.Pipeline;

namespace PromptEnhancer.Services.ChatHistoryService
{
    /// <summary>
    /// Provides utility methods for managing and interacting with chat history.
    /// </summary>
    public interface IChatHistoryService
    {
        /// <summary>
        /// Adds the current user prompt and, if necessary, the system prompt to the chat history.
        /// </summary>
        /// <remarks>If the chat history is empty and a system prompt is provided, the system prompt is
        /// added to the history before the user prompt. This ensures that the system prompt is included in the
        /// conversation context.</remarks>
        /// <param name="context">The pipeline run context containing the chat history and prompts.</param>
        /// <returns>A list of <see cref="ChatMessage"/> objects representing the updated chat history.</returns>
        public List<ChatMessage> CreateChatHistoryFromPipelineRun(PipelineRun context);

        /// <summary>
        /// Calculates the total length of all text messages in the chat history.
        /// </summary>
        /// <param name="history">The collection of chat messages to evaluate. Cannot be null.</param>
        /// <returns>The sum of the lengths of all text messages in the provided chat history.</returns>
        public int GetHistoryLength(IEnumerable<ChatMessage> history);

        /// <summary>
        /// Gets the length of the text contained in the specified chat message.
        /// </summary>
        /// <param name="message">The <see cref="ChatMessage"/> whose text length is to be calculated. Cannot be <c>null</c>.</param>
        /// <returns>The number of characters in the <paramref name="message"/>'s text.</returns>
        public int GetMessageLength(ChatMessage message);

        /// <summary>
        /// Generates a message indicating that the input size limit has been exceeded.
        /// </summary>
        /// <param name="source">The source identifier to include in the message, typically representing the context or component where the
        /// limit was exceeded.</param>
        /// <returns>A formatted string message indicating the input size limit has been exceeded, including the specified
        /// source.</returns>
        public string GetInputSizeExceededLimitMessage(string source);

        /// <summary>
        /// Creates an error indicating that the input size has exceeded the allowed limit.
        /// </summary>
        /// <param name="source">The source identifier to include in the message, typically representing the context or component where the
        /// limit was exceeded.</param>
        /// <returns>An <see cref="Error"/> object representing the failure due to input size exceeding the limit.</returns>
        public Error GetInputSizeExceededLimitError(string source);

        /// <summary>
        /// Adds token usage information to the specified pipeline run context based on the provided input messages,
        /// prompt, and response. Prompt is prioritized over input messages when calculating token usage.
        /// </summary>
        /// <param name="context">The pipeline run context to which token usage information will be added. Cannot be <c>null</c>.</param>
        /// <param name="inputMessages">The collection of input chat messages used in the pipeline run. Cannot be <c>null</c>.</param>
        /// <param name="prompt">An optional prompt string associated with the pipeline run. May be <c>null</c> if no prompt was used.</param>
        /// <param name="response">An optional chat response object representing the output of the pipeline run. May be <c>null</c> if no
        /// response is available.</param>
        public void AddTokenUsageToPipelineRunContext(PipelineRun context, IEnumerable<ChatMessage> inputMessages, string? prompt = null, ChatResponse? response = null);
    }
}
