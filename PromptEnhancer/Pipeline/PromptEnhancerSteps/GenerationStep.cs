using ErrorOr;
using Microsoft.Extensions.AI;
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
            //"""
            //SystemPrompt: You will receive a question that will be used as a search query, try to logically split the question so that
            // search can find relevant data for it. Split the parts by ';' and order them by priority.
            //"""
            var history = context.Entry?.EntryChatHistory?.ToList() ?? new List<ChatMessage>();
            // if empty, add system role from prompt config, if not empty, add user role
            history.Add(new ChatMessage(ChatRole.User, context.UserPromptToLLM));

            var chatClient = settings.Kernel.GetRequiredService<IChatClient>(settings.ChatClientKey);
            var res = chatClient.GetResponseAsync(history, settings.ChatOptions, cancellationToken);
            return true;
        }

        protected override ErrorOr<bool> CheckExecuteConditions(PipelineContext context)
        {
            if (!string.IsNullOrEmpty(context.QueryString) || !string.IsNullOrEmpty(context.UserPromptToLLM) || !string.IsNullOrEmpty(context.Entry?.QueryString))
            {
                return true;
            }

            return FailCondition();
        }
    }
}
