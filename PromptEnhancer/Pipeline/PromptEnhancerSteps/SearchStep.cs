using ErrorOr;
using PromptEnhancer.KnowledgeBaseCore.Interfaces;
using PromptEnhancer.KnowledgeRecord.Interfaces;
using PromptEnhancer.KnowledgeSearchRequest;
using PromptEnhancer.Models.Pipeline;

namespace PromptEnhancer.Pipeline.PromptEnhancerSteps
{
    /// <summary>
    /// Represents a pipeline step that performs a knowledge base search using the specified search request, filter, and
    /// settings. For more control, multiple search KBs use other step: MultipleSearchStep.
    /// </summary>
    /// <remarks>This step executes a search operation against a knowledge base and retrieves a specified
    /// maximum number of records. The search can be customized using the provided search request, filter, and settings.
    /// The retrieved records are added to the pipeline run context for further processing.</remarks>
    /// <typeparam name="TRecord">The type of the knowledge record being retrieved.</typeparam>
    /// <typeparam name="TSearchFilter">The type of the search filter used to refine the search.</typeparam>
    /// <typeparam name="TSearchSettings">The type of the search settings used to configure the search operation.</typeparam>
    /// <typeparam name="TFilter">The type of the model filter applied to the search results.</typeparam>
    /// <typeparam name="T">The type of the model being filtered.</typeparam>
    public class SearchStep<TRecord, TSearchFilter, TSearchSettings, TFilter, T> : PipelineStep
        where TRecord : class, IKnowledgeRecord
        where TSearchFilter : class, IKnowledgeBaseSearchFilter
        where TSearchSettings : class, IKnowledgeBaseSearchSettings
        where TFilter : class, IModelFilter<T>
        where T : class
    {
        private readonly string? _knowledgeBaseKey = null;
        private readonly KnowledgeSearchRequest<TSearchFilter, TSearchSettings> _request;
        private readonly TFilter? _filter;
        private readonly int _maxRecords;

        public SearchStep(KnowledgeSearchRequest<TSearchFilter, TSearchSettings> request, TFilter? filter = null, int maxRecords = 50, string? knowledgeBaseKey = null, bool isRequired = false) : base(isRequired)
        {
            _knowledgeBaseKey = knowledgeBaseKey;
            _request = request;
            _filter = filter;
            _maxRecords = maxRecords;
        }

        /// <inheritdoc/>
        /// <remarks>This method retrieves records from a knowledge base based on the provided query
        /// strings and filter criteria, and adds the retrieved records to the pipeline context. The maximum number of
        /// records retrieved is limited by the step's configuration.</remarks>
        protected async override Task<ErrorOr<bool>> ExecuteStepAsync(PipelineSettings settings, PipelineRun context, CancellationToken cancellationToken = default)
        {
            try
            {
                List<IKnowledgeRecord> recordsToAdd = [];
                var kernel = settings.Kernel;
                var kb = settings.GetService<IKnowledgeBase<TRecord, TSearchFilter, TSearchSettings, TFilter, T>>(_knowledgeBaseKey);
                var res = await kb!.SearchAsync(_request, context.QueryStrings.Any() ? context.QueryStrings : [context.QueryString!], _filter, cancellationToken);
                recordsToAdd.AddRange(res.Take(_maxRecords));

                context.RetrievedRecords.AddRange(recordsToAdd);
                return true;
            }
            catch (Exception ex)
            {
                return Error.Failure($"{GetType()}: Exception during search execution - {ex.Message}");
            }
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
