using ErrorOr;
using Microsoft.Extensions.AI;
using PromptEnhancer.AIUtility.ChatHistory;
using PromptEnhancer.AIUtility.TokenCountFallback;
using PromptEnhancer.Models.Pipeline;

namespace PromptEnhancer.Pipeline.PromptEnhancerSteps
{
    public class GenerationStep : PipelineStep
    {
        public GenerationStep(bool isRequired = false) : base(isRequired) { }
        protected async override Task<ErrorOr<bool>> ExecuteStepAsync(PipelineSettings settings, PipelineRun context, CancellationToken cancellationToken = default)
        {
            var chatClient = settings.Kernel.GetRequiredService<IChatClient>(settings.Settings.ChatClientKey);
            List<ChatMessage> history = ChatHistoryUtility.AddToChatHistoryPipeline(context);
            if (ChatHistoryUtility.GetHistoryLength(history) > settings.Settings.MaximumInputLength)
            {
                return FailExecution(ChatHistoryUtility.GetInputSizeExceededLimitMessage(GetType().Name));
            }

            var res = await chatClient.GetResponseAsync(history, settings.Settings.ChatOptions, cancellationToken);
            context.InputTokenUsage += res.Usage?.InputTokenCount ?? TokenCounter.CountTokens(history); //TODO do some custom counter, if the method ends in success but still 0? - make it in other parts of code too
            context.OutputTokenUsage += res.Usage?.OutputTokenCount ?? TokenCounter.CountTokens(res.Messages);
            context.FinalResponse = res;
            history.AddRange(res.Messages);
            context.ChatHistory = history;
            return true;
        }

        protected override ErrorOr<bool> CheckExecuteConditions(PipelineRun context)
        {
            if (!string.IsNullOrEmpty(context.QueryString) || !string.IsNullOrEmpty(context.UserPromptToLLM))
            {
                return true;
            }

            return FailCondition();
        }
    }
}
