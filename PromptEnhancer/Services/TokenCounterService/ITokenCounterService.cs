using Microsoft.Extensions.AI;

namespace PromptEnhancer.Services.TokenCounterService
{
    /// <summary>
    /// Provides methods for counting tokens in chat messages or text strings using a specified encoding.
    /// </summary>
    public interface ITokenCounterService
    {
        /// <summary>
        /// Calculates the total number of tokens in a collection of chat messages.
        /// </summary>
        /// <param name="messages">A collection of <see cref="ChatMessage"/> objects whose tokens are to be counted. Cannot be null.</param>
        /// <returns>The total number of tokens across all chat messages in the collection.</returns>
        public int CountTokens(IEnumerable<ChatMessage> messages);

        /// <summary>
        /// Counts the number of tokens in the specified message.
        /// </summary>
        /// <param name="message">The message to be tokenized and counted. Cannot be null.</param>
        /// <returns>The total number of tokens in the message.</returns>
        public int CountTokens(string message);
    }
}
