using ErrorOr;
using Microsoft.Extensions.AI;
using PromptEnhancer.Models.Pipeline;

namespace PromptEnhancer.Pipeline.PromptEnhancerSteps
{
    public class QueryParserStep : PipelineStep
    {
        public const string PromptTemplate =
            """
            Given the user query "{0}", extract up to "{1}" meaningful and relevant search queries, separated by ';'.

            Keep only what is useful for search. Context for each part must be clear (you can repeat some words from query).
            Remove filler or irrelevant parts. If the query can’t be improved or split, return "{2}".
            Example:
            “What is the average size of a beehive, and I also wanted to ask what time it is?” -> “average size of a beehive;what time is it?”
            
            """;

        public const string FailResponseLLM = "NaN";

        public const int MaxResponseLength = 200;

        private readonly int _maxSplit;
        private readonly ChatOptions? _options;

        public QueryParserStep(int maxSplit = 3, ChatOptions? options = null, bool isRequired = false) : base(isRequired)
        {
            _maxSplit = maxSplit;
            _options = options;
        }
        protected async override Task<ErrorOr<bool>> ExecuteStepAsync(PipelineSettings settings, PipelineContext context, CancellationToken cancellationToken = default)
        {
            //TODO maybe more checks for the llm response?
            var chatClient = settings.Kernel.GetRequiredService<IChatClient>(settings.Settings.ChatClientKey);
            var res = await chatClient.GetResponseAsync(string.Format(PromptTemplate, context.QueryString, _maxSplit, FailResponseLLM), _options ?? settings.Settings.ChatOptions, cancellationToken: cancellationToken);
            if (res.Text == FailResponseLLM || res.Text.Length > MaxResponseLength || res.Text.Count(c => c == ';') > _maxSplit)
            {
                return false;
            }
            context.InputTokenUsage = res.Usage?.InputTokenCount ?? 0;
            context.OutputTokenUsage = res.Usage?.OutputTokenCount ?? 0;
            context.QueryStrings = res.Text.Split(';').Select(x => x.Trim()).Where(x => !string.IsNullOrEmpty(x));

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
