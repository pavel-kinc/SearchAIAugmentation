using ErrorOr;
using Microsoft.Extensions.AI;
using PromptEnhancer.Models.Configurations;
using PromptEnhancer.Models.Pipeline;

namespace PromptEnhancer.AIUtility.ChatHistory
{
    public static class ChatHistoryUtility
    {
        public static List<ChatMessage> AddToChatHistoryPipeline(PipelineContext context)
        {
            var history = context.ChatHistory?.ToList() ?? [];

            // if empty, add system role from prompt config, if not empty, add user role
            if (context.SystemPromptToLLM is not null && !history.Any(x => x.Role == ChatRole.System))
            {
                history.Add(new ChatMessage(ChatRole.System, context.SystemPromptToLLM));
            }
            history.Add(new ChatMessage(ChatRole.User, context.UserPromptToLLM));
            return history;
        }

        public static int GetHistoryLength(IEnumerable<ChatMessage> history)
        {
            return history.Sum(x => x.Text.Length);
        }

        public static string GetInputSizeExceededLimitMessage(string source)
        {
            return $"{source}: chat history exceeded input size limit. You can modify it in {nameof(PipelineAdditionalSettings)} for {nameof(EnhancerConfiguration)}.";
        }

        public static Error GetInputSizeExceededLimitError(string source)
        {
            return Error.Failure(GetInputSizeExceededLimitMessage(source));
        }
    }
}
