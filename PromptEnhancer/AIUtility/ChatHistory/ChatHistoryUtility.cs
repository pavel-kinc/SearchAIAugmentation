using Microsoft.Extensions.AI;
using PromptEnhancer.Models.Pipeline;

namespace PromptEnhancer.AIUtility.ChatHistory
{
    public static class ChatHistoryUtility
    {
        public static List<ChatMessage> AddToChatHistoryPipeline(PipelineContext context)
        {
            var history = context.Entry?.EntryChatHistory?.ToList() ?? [];

            // if empty, add system role from prompt config, if not empty, add user role
            if (context.SystemPromptToLLM is not null && !history.Any(x => x.Role == ChatRole.System))
            {
                history.Add(new ChatMessage(ChatRole.System, context.SystemPromptToLLM));
            }
            history.Add(new ChatMessage(ChatRole.User, context.UserPromptToLLM));
            return history;
        }
    }
}
