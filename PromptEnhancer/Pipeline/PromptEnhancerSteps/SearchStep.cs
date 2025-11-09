using ErrorOr;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualBasic;
using PromptEnhancer.KnowledgeBase;
using PromptEnhancer.KnowledgeBase.Interfaces;
using PromptEnhancer.KnowledgeRecord.Interfaces;
using PromptEnhancer.KnowledgeSearchRequest;
using PromptEnhancer.Models.Pipeline;
using PromptEnhancer.Plugins;
using PromptEnhancer.Plugins.Interfaces;
using System.Collections.Generic;

namespace PromptEnhancer.Pipeline.PromptEnhancerSteps
{
    public class SearchStep<TRecord, TSearchFilter, TSearchSettings, TFilter> : PipelineStep
        where TRecord : class, IKnowledgeRecord
        where TSearchFilter : class, IKnowledgeBaseSearchFilter
        where TSearchSettings : class, IKnowledgeBaseSearchSettings
        where TFilter : class, IRecordFilter?
    {
        private readonly string? _knowledgeBaseKey = null;
        private readonly KnowledgeSearchRequest<TSearchFilter, TSearchSettings> _request;
        private readonly TFilter? _filter;

        public SearchStep(KnowledgeSearchRequest<TSearchFilter, TSearchSettings> request, TFilter? filter = null, string? knowledgeBaseKey = null, bool isRequired = false)
        {
            // is key needed here? should i put knowledge base here directly?
            _knowledgeBaseKey = knowledgeBaseKey;
            _isRequired = isRequired;
            _request = request;
            _filter = filter;
        }

        protected async override Task<ErrorOr<bool>> ExecuteStepAsync(PipelineSettings settings, PipelineContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                List<IKnowledgeRecord> recordsToAdd = [];
                var kernel = settings.Kernel;
                //TODO plugins in each step? how to choose, how to setup needed ones
                var kb = settings.GetService<IKnowledgeBase<TRecord, TSearchFilter, TSearchSettings, TFilter>>(_knowledgeBaseKey);
                var searchReq = new KnowledgeSearchRequest<TSearchFilter, TSearchSettings>
                {
                    Filter = _request.Filter,
                    Settings = _request.Settings,
                    //QueryString = context.QueryString!,
                };
                var res = await kb!.SearchAsync(searchReq, context, _filter, cancellationToken);
                recordsToAdd.AddRange(res);

                context.RetrievedRecords.AddRange(recordsToAdd);
                //TODO work with knowledge bases and processor
                return recordsToAdd.Count != 0;
            }
            catch (Exception ex)
            {
                return Error.Failure($"{GetType()}: Exception during search execution - {ex.Message}");
            }
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
}
