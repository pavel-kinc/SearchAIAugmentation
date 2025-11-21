using Microsoft.Extensions.AI;
using SharpToken;

namespace PromptEnhancer.AIUtility.TokenCountFallback
{
    public static class TokenCounter
    {
        public const string Encoding = "cl100k_base";

        public static int CountTokens(IEnumerable<ChatMessage> messages)
        {
            int tokens = 0;
            foreach (var message in messages)
            {
                tokens += message.Text.Length;
            }
            return tokens;
        }
        public static int CountTokens(string message)
        {
            var encoding = GptEncoding.GetEncoding(Encoding);
            return encoding.Encode(message).Count;
        }
    }
}
