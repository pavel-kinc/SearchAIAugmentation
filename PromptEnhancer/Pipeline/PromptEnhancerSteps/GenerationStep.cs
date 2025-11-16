using ErrorOr;
using Microsoft.Extensions.AI;
using PromptEnhancer.AIUtility.ChatHistory;
using PromptEnhancer.Models.Pipeline;

namespace PromptEnhancer.Pipeline.PromptEnhancerSteps
{
    //TODO FINISH
    public class GenerationStep : PipelineStep
    {
        public GenerationStep(bool isRequired = false) : base(isRequired) { }
        //TODO for the streaming result, use other class completely i guess (aka dont use pipeline or give this step public method?)
        protected async override Task<ErrorOr<bool>> ExecuteStepAsync(PipelineSettings settings, PipelineContext context, CancellationToken cancellationToken = default)
        {
            var chatClient = settings.Kernel.GetRequiredService<IChatClient>(settings.Settings.ChatClientKey);
            List<ChatMessage> history = ChatHistoryUtility.AddToChatHistoryPipeline(context);

            var res = await chatClient.GetResponseAsync(history, settings.Settings.ChatOptions, cancellationToken);
            context.InputTokenUsage += res.Usage?.InputTokenCount ?? 0; //TODO do some custom counter, if the method ends in success but still 0?
            context.OutputTokenUsage += res.Usage?.OutputTokenCount ?? 0;
            context.FinalResponse = res;
            return true;
        }

        protected override ErrorOr<bool> CheckExecuteConditions(PipelineContext context)
        {
            if (!string.IsNullOrEmpty(context.QueryString) || !string.IsNullOrEmpty(context.UserPromptToLLM))
            {
                return true;
            }

            return FailCondition();
        }
    }
}
