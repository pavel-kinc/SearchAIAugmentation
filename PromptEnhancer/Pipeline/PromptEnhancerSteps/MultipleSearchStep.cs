using ErrorOr;
using Microsoft.Extensions.AI;
using PromptEnhancer.KnowledgeBaseCore.Interfaces;
using PromptEnhancer.KnowledgeRecord.Interfaces;
using PromptEnhancer.Models.Pipeline;
using PromptEnhancer.Services.ChatHistoryService;
using System.Collections.Concurrent;

namespace PromptEnhancer.Pipeline.PromptEnhancerSteps
{
    /// <summary>
    /// Represents a pipeline step that performs a multi-source search operation to retrieve relevant knowledge records
    /// based on a user query.
    /// </summary>
    /// <remarks>This step interacts with multiple knowledge base containers to search for records that match
    /// the user query.  It allows for automatic selection of knowledge bases or manual selection based on user-defined
    /// criteria.  The retrieved records are added to the pipeline context for further processing.  The step ensures
    /// that a minimum number of records are retrieved and supports parallel execution for improved performance. If the
    /// number of retrieved records is below the specified threshold, the step fails.</remarks>
    public class MultipleSearchStep : PipelineStep
    {
        public const char Separator = ';';
        const string prompt = """
                              User search query:
                              {0}

                              Candidate objects (ID and Description):
                              {1}

                              Pick all object IDs (integers) whose description can help find relevant information for the user search query.
                              Do not pick any objects, that have very little relevance to the user search query.
                              Return only a string of the IDs separated by this char ';' no newline. Try to pick atleast {2} object IDs.
                              If none are relevant or cannot be picked, return an empty string, no newline.
                              """;

        private readonly IEnumerable<IKnowledgeBaseContainer> _knowledgeBases;
        private readonly int _atleastToPick;
        private readonly int _minimumRecordsToRetrieve;
        private readonly bool _allowAutoChoice;
        private readonly string? _additionalInstructions;
        private readonly int _maxRecords;

        public MultipleSearchStep(IEnumerable<IKnowledgeBaseContainer> knowledgeBases, int atleastKBsToPick = 0, int minimumRecordsToRetrieve = 0, bool allowAutoChoice = true, string? additionalChoiceInstructions = null, int maxRecordsPerKB = 50, bool isRequired = false) : base(isRequired)
        {
            _knowledgeBases = knowledgeBases;
            _atleastToPick = atleastKBsToPick;
            _minimumRecordsToRetrieve = minimumRecordsToRetrieve;
            _allowAutoChoice = allowAutoChoice;
            _additionalInstructions = additionalChoiceInstructions;
            _maxRecords = maxRecordsPerKB;
        }

        /// <inheritdoc/>
        /// <remarks>This method selects knowledge bases based on the provided settings and performs
        /// parallel searches  using the query strings from the pipeline context. The retrieved records are added to the
        /// context  if the minimum required number of records is met. If the operation fails due to insufficient
        /// records  or other conditions, an error is returned.</remarks>
        protected async override Task<ErrorOr<bool>> ExecuteStepAsync(PipelineSettings settings, PipelineRun context, CancellationToken cancellationToken = default)
        {
            try
            {
                var pickedBases = _allowAutoChoice ? await PickKnowledgeBases(_knowledgeBases, settings, context, cancellationToken) : _knowledgeBases;
                if (pickedBases is null)
                {
                    var chatHistoryService = settings.GetService<IChatHistoryService>();
                    return FailExecution(chatHistoryService.GetInputSizeExceededLimitMessage(GetType().Name));
                }
                if (pickedBases.Count() < _atleastToPick)
                {
                    return false;
                }
                var cb = new ConcurrentBag<IKnowledgeRecord>();

                await Parallel.ForEachAsync(pickedBases, async (kb, _) =>
                {
                    var results = (await kb.SearchAsync(context.QueryStrings.Any() ? context.QueryStrings : [context.QueryString!], cancellationToken)).Take(_maxRecords);
                    foreach (var item in results)
                    {
                        cb.Add(item);
                    }

                });
                if (cb.Count < _minimumRecordsToRetrieve)
                {
                    return FailExecution($"Error: {GetType().Name}, retrieved record count - {cb.Count} was smaller than minimum - {_minimumRecordsToRetrieve}");
                }
                context.RetrievedRecords.AddRange(cb);

                return true;
            }
            catch (Exception ex)
            {
                return Error.Failure($"{GetType().Name}: Exception during search execution - {ex.Message}", ex.Message);
            }
        }

        /// <summary>
        /// Allows the user to select a subset of knowledge bases from the provided collection based on a generated
        /// prompt.
        /// </summary>
        /// <remarks>This method generates a prompt based on the provided knowledge bases and user query,
        /// then uses a chat client to obtain a response. The response is parsed to determine the indices of the
        /// selected knowledge bases. The caller is responsible for handling any exceptions that may occur during the
        /// deserialization of indices.</remarks>
        /// <param name="knowledgeBases">The collection of knowledge bases available for selection.</param>
        /// <param name="settings">The pipeline settings that provide configuration and dependencies for the operation.</param>
        /// <param name="context">The current pipeline run context, which includes query information and token usage tracking.</param>
        /// <param name="ct">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the selected knowledge bases  as
        /// an enumerable collection, or <see langword="null"/> if the prompt exceeds the maximum input length or  no
        /// valid selection is made.</returns>
        protected virtual async Task<IEnumerable<IKnowledgeBaseContainer>?> PickKnowledgeBases(IEnumerable<IKnowledgeBaseContainer> knowledgeBases, PipelineSettings settings, PipelineRun context, CancellationToken ct)
        {
            var chatClient = settings.Kernel.GetRequiredService<IChatClient>(settings.Settings.ChatClientKey);
            var chatHistoryService = settings.GetService<IChatHistoryService>();
            var picking = string.Join(Environment.NewLine, knowledgeBases.Select((kb, i) => $"{i}: {kb.Description}"));
            var choicePrompt = string.Format(prompt, context.QueryString!, string.IsNullOrWhiteSpace(picking) ? "Nothing to pick from" : picking, _atleastToPick);
            choicePrompt = choicePrompt + (string.IsNullOrWhiteSpace(_additionalInstructions) ? string.Empty : $"{Environment.NewLine}Additional user instructions:{Environment.NewLine}{_additionalInstructions}");
            if (prompt.Length > settings.Settings.MaximumInputLength)
            {
                return null;
            }
            var res = await chatClient.GetResponseAsync(choicePrompt, settings.Settings.ChatOptions, ct);
            AssignTokensToContext(context, chatHistoryService, prompt: choicePrompt, response: res);

            // Filter the list by the deserialized indices; caller handles any exceptions.
            var resultText = res.Text.Trim();
            var ids = !string.IsNullOrEmpty(resultText) && resultText.All(x => char.IsDigit(x) || x == Separator) ? resultText.Split(Separator).Select(x => int.Parse(x)) : [];
            IEnumerable<IKnowledgeBaseContainer> picked = ids!
                .Where(i => i >= 0 && i < knowledgeBases.Count())
                .Select(i => knowledgeBases.ElementAt(i));
            return picked;
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
