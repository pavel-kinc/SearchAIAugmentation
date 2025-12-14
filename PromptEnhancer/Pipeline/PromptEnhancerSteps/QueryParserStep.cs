using ErrorOr;
using Microsoft.Extensions.AI;
using PromptEnhancer.Models.Pipeline;
using PromptEnhancer.Services.ChatHistoryService;

namespace PromptEnhancer.Pipeline.PromptEnhancerSteps
{
    /// <summary>
    /// Represents a pipeline step that processes a user query to extract meaningful and distinct search queries.
    /// </summary>
    /// <remarks>This step uses a language model to analyze the user query and generate up to a specified
    /// number of distinct search queries.  If the query cannot be improved or split, a predefined failure response is
    /// returned. The step ensures that the generated  queries are relevant to the original user query and do not exceed
    /// the maximum allowed response length.</remarks>
    public class QueryParserStep : PipelineStep
    {
        /// <summary>
        /// A template string used to generate a prompt for extracting meaningful and distinct search queries from a
        /// user query.
        /// </summary>
        public const string PromptTemplate =
            """
            Given the user query "{0}", extract up to "{1}" meaningful and distinct search queries, separated by ';'.

            From the provided user query, keep only the parts that are useful for search.  
            Each retained part must have a clear relationship to the user’s query (you can repeat some query keywords if needed), but the parts must remain distinct from each other.  
            Do not include any extraneous information that is not directly relevant to search.
            If the query can’t be improved or split, return "{2}".
            """;

        public const string FailResponseLLM = "NaN";

        public const int MaxResponseLength = 200;

        public const char Delimiter = ';';

        private readonly int _maxSplit;
        private readonly ChatOptions? _options;

        public QueryParserStep(int maxSplit = 3, ChatOptions? options = null, bool isRequired = false) : base(isRequired)
        {
            _maxSplit = maxSplit;
            _options = options;
        }

        /// <inheritdoc/>
        /// <remarks>This method interacts with a chat client to process the input query and generate a
        /// response. It validates  the input prompt and response to ensure they meet the configured constraints, such
        /// as maximum length and  formatting rules. If the input or response violates these constraints, the step fails
        /// with an appropriate error.</remarks>
        protected async override Task<ErrorOr<bool>> ExecuteStepAsync(PipelineSettings settings, PipelineRun context, CancellationToken cancellationToken = default)
        {
            //TODO maybe more checks for the llm response?
            var chatClient = settings.Kernel.GetRequiredService<IChatClient>(settings.Settings.ChatClientKey);
            var chatHistoryService = settings.GetService<IChatHistoryService>();
            var inputPrompt = string.Format(PromptTemplate, context.QueryString, _maxSplit, FailResponseLLM);
            if (inputPrompt.Length > settings.Settings.MaximumInputLength)
            {
                return FailExecution(chatHistoryService.GetInputSizeExceededLimitMessage(GetType().Name));
            }
            var res = await chatClient.GetResponseAsync(inputPrompt, _options ?? settings.Settings.ChatOptions, cancellationToken: cancellationToken);
            AssignTokensToContext(context, chatHistoryService, prompt: inputPrompt, response: res);
            if (res.Text == FailResponseLLM || res.Text.Length > MaxResponseLength || res.Text.Count(c => c == Delimiter) >= _maxSplit)
            {
                return FailExecution();
            }
            context.QueryStrings = res.Text.Split(Delimiter).Select(x => x.Trim()).Where(x => !string.IsNullOrEmpty(x) && !x.Equals(FailResponseLLM));

            return true;
        }

        /// <inheritdoc/>
        protected override ErrorOr<bool> CheckExecutionConditions(PipelineRun context)
        {
            if (!string.IsNullOrEmpty(context.QueryString))
            {
                return true;
            }

            return FailCondition();
        }
    }
}
