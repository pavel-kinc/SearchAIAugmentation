using ErrorOr;
using Microsoft.Extensions.AI;
using PromptEnhancer.Models.Configurations;
using PromptEnhancer.Models.Pipeline;
using PromptEnhancer.Services.TokenCounterService;

namespace PromptEnhancer.Services.ChatHistoryService
{
    /// <summary>
    /// Provides utility methods for managing and interacting with chat history.
    /// </summary>
    /// <remarks>This static class includes methods to add messages to a chat history, calculate the total
    /// length of chat messages, and generate error messages related to input size limits. It is designed to be used in
    /// the context of a pipeline run, where chat history management is required.</remarks>
    public class ChatHistoryService : IChatHistoryService
    {
        private readonly ITokenCounterService _tokenCounterService;

        public ChatHistoryService(ITokenCounterService tokenCounterService)
        {
            _tokenCounterService = tokenCounterService;
        }
        public List<ChatMessage> CreateChatHistoryFromPipelineRun(PipelineRun context)
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

        public int GetHistoryLength(IEnumerable<ChatMessage> history)
        {
            return history.Sum(x => x.Text.Length);
        }

        public int GetMessageLength(ChatMessage message)
        {
            return message.Text.Length;
        }

        public string GetInputSizeExceededLimitMessage(string source)
        {
            return $"{source}: chat history exceeded input size limit. You can modify it in {nameof(PipelineAdditionalSettings)} for {nameof(EnhancerConfiguration)}.";
        }

        public Error GetInputSizeExceededLimitError(string source)
        {
            return Error.Failure(GetInputSizeExceededLimitMessage(source));
        }

        public void AddTokenUsageToPipelineRunContext(PipelineRun context, IEnumerable<ChatMessage> inputMessages, string? prompt = null, ChatResponse? response = null)
        {
            context.InputTokenUsage += response?.Usage?.InputTokenCount ?? (prompt is null ? _tokenCounterService.CountTokens(inputMessages) : _tokenCounterService.CountTokens(prompt));
            context.OutputTokenUsage += response?.Usage?.OutputTokenCount ?? _tokenCounterService.CountTokens(response?.Messages ?? []);
        }
    }
}
