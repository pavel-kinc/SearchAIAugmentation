using ErrorOr;
using Microsoft.Extensions.AI;
using PromptEnhancer.KnowledgeBaseCore.Interfaces;
using PromptEnhancer.KnowledgeRecord.Interfaces;
using PromptEnhancer.Models.Pipeline;
using System.Collections.Concurrent;

namespace PromptEnhancer.Pipeline.PromptEnhancerSteps
{
    //TODO maybe make convenience classes for less generics without required generics (like filters and settings)
    public class MultipleSearchStep : PipelineStep
    {
        const string prompt = """
                              User search query:
                              {0}

                              Candidate objects (ID and Description):
                              {1}

                              Pick all object IDs (integers) whose description can help find relevant information for the user search query.
                              Return only a string of the IDs separated by this char ';'. Try to pick atleast {2} object IDs.
                              If none are relevant or cannot be picked, return an empty list.
                              """;

        private readonly IEnumerable<IKnowledgeBaseContainer> _knowledgeBases;
        private readonly int _atleastToPick;
        private readonly bool _allowAutoChoice;
        private readonly string? _additionalInstructions;
        private readonly int _maxRecords;

        public MultipleSearchStep(IEnumerable<IKnowledgeBaseContainer> knowledgeBases, int atleastToPick = 0, bool allowAutoChoice = true, string? additionalChoiceInstructions = null, int maxRecordsPerKB = 50, bool isRequired = false) : base(isRequired)
        {
            _knowledgeBases = knowledgeBases;
            _atleastToPick = atleastToPick;
            _allowAutoChoice = allowAutoChoice;
            _additionalInstructions = additionalChoiceInstructions;
            _maxRecords = maxRecordsPerKB;
        }

        protected async override Task<ErrorOr<bool>> ExecuteStepAsync(PipelineSettings settings, PipelineContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var pickedBases = _allowAutoChoice ? await PickKnowledgeBases(_knowledgeBases, settings, context, cancellationToken) : _knowledgeBases;
                if (pickedBases.Count() < _atleastToPick)
                {
                    return false;
                }
                var cb = new ConcurrentBag<IKnowledgeRecord>();

                await Parallel.ForEachAsync(pickedBases, async (kb, _) =>
                {
                    //TODO if search fails for 1, catch exception and continue?
                    var results = (await kb.SearchAsync(context.QueryStrings.Any() ? context.QueryStrings : [context.QueryString!], cancellationToken)).Take(_maxRecords);
                    foreach (var item in results)
                    {
                        cb.Add(item);
                    }
                        
                });
                context.RetrievedRecords.AddRange(cb);

                return !cb.IsEmpty;
            }
            catch (Exception ex)
            {
                return Error.Failure($"{GetType()}: Exception during search execution - {ex.Message}", ex.Message);
            }
        }

        protected virtual async Task<IEnumerable<IKnowledgeBaseContainer>> PickKnowledgeBases(IEnumerable<IKnowledgeBaseContainer> knowledgeBases, PipelineSettings settings, PipelineContext context, CancellationToken ct)
        {
            var chatClient = settings.Kernel.GetRequiredService<IChatClient>(settings.Settings.ChatClientKey);
            var picking = string.Join(Environment.NewLine, knowledgeBases.Select((kb, i) => $"{i}: {kb.Description}"));
            var choicePrompt = string.Format(prompt, context.QueryString!, string.IsNullOrWhiteSpace(picking) ? "Nothing to pick from" : picking, _atleastToPick);
            choicePrompt = choicePrompt + (string.IsNullOrWhiteSpace(_additionalInstructions) ? string.Empty : $"{Environment.NewLine}Additional user instructions:{Environment.NewLine}{_additionalInstructions}");
            var res = await chatClient.GetResponseAsync(choicePrompt, settings.Settings.ChatOptions, ct);
            context.InputTokenUsage += res.Usage?.InputTokenCount ?? 0;
            context.OutputTokenUsage += res.Usage?.OutputTokenCount ?? 0;

            // Filter the list by the deserialized indices; caller handles any exceptions.
            var ids = res.Text.Trim().Split(';').Select(x => int.Parse(x));
            IEnumerable<IKnowledgeBaseContainer> picked = ids!
                .Where(i => i >= 0 && i < knowledgeBases.Count())
                .Select(i => knowledgeBases.ElementAt(i));
            return picked;
        }

        protected override ErrorOr<bool> CheckExecuteConditions(PipelineContext context)
        {
            if (!string.IsNullOrEmpty(context.QueryString))
            {
                return true;
            }

            return FailCondition();
        }


        //private DateTimePlugin? GetDateTimePlugin(Microsoft.SemanticKernel.Kernel kernel)
        //{
        //    var plugin = kernel.Plugins.TryGetPlugin<DateTimePlugin>(nameof(DateTimePlugin));
        //    return plugin;
        //}
    }

    //TODO replace interfaces with "empty" base classes if needed
    //public class SearchStep<TRecord, T> : SearchStep<TRecord, IKnowledgeBaseSearchFilter, IKnowledgeBaseSearchSettings, IRecordFilter<T>, T>
    //where TRecord : class, IKnowledgeRecord
    //where T : class
    //{ }
}
