using ErrorOr;
using Microsoft.Extensions.AI;
using PromptEnhancer.Models.Pipeline;
using System.Threading;

namespace PromptEnhancer.Pipeline.PromptEnhancerSteps
{
    public class QueryParserStep : PipelineStep
    {
        public const string PromptTemplate =
            """
            SystemPrompt: You will receive a question that will be used as a search query, try to logically split the question into maximally {0} search precise queries
            so that search can find relevant data for the unique search query part. Split the parts by ';' and order them by priority. If you cannot do it, return "{1}"
            """;

        public const string FailResponseLLM = "NaN";

        private readonly string? _chatClientKey;
        private readonly int _maxSplit;
        private readonly ChatOptions? _options;

        public QueryParserStep(string? chatClientKey = null, int maxSplit = 3, ChatOptions? options = null, bool isRequired = false) : base(isRequired)
        {
            _chatClientKey = chatClientKey;
            _maxSplit = maxSplit;
            _options = options;
        }
        protected async override Task<ErrorOr<bool>> ExecuteStepAsync(PipelineSettings settings, PipelineContext context, CancellationToken cancellationToken = default)
        {
            //TODO maybe more checks for the llm response?
            var chatClient = settings.Kernel.GetRequiredService<IChatClient>(_chatClientKey);
            var res = await chatClient.GetResponseAsync(string.Format(PromptTemplate, _maxSplit, FailResponseLLM), _options ?? settings.ChatOptions, cancellationToken: cancellationToken);
            if(res.Text == FailResponseLLM)
            {
                return false;
            }

            context.QueryStrings = res.Text.Split(';').Select(x => x.Trim());

            return true;
        }

        protected override ErrorOr<bool> CheckExecuteConditions(PipelineContext context)
        {
            if (!string.IsNullOrEmpty(context.QueryString))
            {
                return true;
            }

            return FailCondition();
        }
    }
}
