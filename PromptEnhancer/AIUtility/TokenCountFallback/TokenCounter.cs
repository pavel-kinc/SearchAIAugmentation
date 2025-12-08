using Microsoft.Extensions.AI;
using SharpToken;

namespace PromptEnhancer.AIUtility.TokenCountFallback
{
    /// <summary>
    /// Provides methods for counting tokens in chat messages or text strings using a specified encoding.
    /// </summary>
    /// <remarks>This class uses the "cl100k_base" encoding to calculate the number of tokens in the input. It
    /// offers methods to count tokens from a collection of <see cref="ChatMessage"/> objects or a single
    /// string.</remarks>
    public static class TokenCounter
    {
        /// <summary>
        /// Represents the encoding type used for data processing.
        /// </summary>
        public const string Encoding = "cl100k_base";

        /// <summary>
        /// Calculates the total number of tokens in a collection of chat messages.
        /// </summary>
        /// <param name="messages">A collection of <see cref="ChatMessage"/> objects whose tokens are to be counted. Cannot be null.</param>
        /// <returns>The total number of tokens across all chat messages in the collection.</returns>
        public static int CountTokens(IEnumerable<ChatMessage> messages)
        {
            int tokens = 0;
            foreach (var message in messages)
            {
                tokens += message.Text.Length;
            }
            return tokens;
        }

        /// <summary>
        /// Counts the number of tokens in the specified message.
        /// </summary>
        /// <param name="message">The message to be tokenized and counted. Cannot be null.</param>
        /// <returns>The total number of tokens in the message.</returns>
        public static int CountTokens(string message)
        {
            var encoding = GptEncoding.GetEncoding(Encoding);
            return encoding.Encode(message).Count;
        }
    }
}
