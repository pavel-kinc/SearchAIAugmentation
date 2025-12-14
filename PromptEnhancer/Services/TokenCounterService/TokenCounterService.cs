using Microsoft.Extensions.AI;
using SharpToken;

namespace PromptEnhancer.Services.TokenCounterService
{
    /// <summary>
    /// Provides methods for counting tokens in chat messages or text strings using a specified encoding.
    /// </summary>
    /// <remarks>This class uses the "cl100k_base" encoding to calculate the number of tokens in the input. It
    /// offers methods to count tokens from a collection of <see cref="ChatMessage"/> objects or a single
    /// string.</remarks>
    public class TokenCounterService : ITokenCounterService
    {
        /// <summary>
        /// Represents the encoding type used for data processing.
        /// </summary>
        public const string Encoding = "cl100k_base";

        public int CountTokens(IEnumerable<ChatMessage> messages)
        {
            int tokens = 0;
            foreach (var message in messages)
            {
                tokens += message.Text.Length;
            }
            return tokens;
        }


        public int CountTokens(string message)
        {
            var encoding = GptEncoding.GetEncoding(Encoding);
            return encoding.Encode(message).Count;
        }
    }
}
