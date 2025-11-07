using ErrorOr;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualBasic;
using PromptEnhancer.KnowledgeBase;
using PromptEnhancer.KnowledgeBase.Interfaces;
using PromptEnhancer.Models.Pipeline;
using PromptEnhancer.Plugins;
using PromptEnhancer.Plugins.Interfaces;

namespace PromptEnhancer.Pipeline.PromptEnhancerSteps
{
    public class SearchStep : PipelineStep
    {
        private readonly string? _knowledgeBaseKey = null;

        public SearchStep(string? knowledgeBaseKey = null)
        {
            _knowledgeBaseKey = knowledgeBaseKey;
            
        }

        protected async override Task<ErrorOr<bool>> ExecuteStepAsync(PipelineSettings settings, PipelineContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                List<KnowledgeRecord> recordsToAdd = [];
                var kernel = settings.Kernel;
                //TODO plugins in each step? how to choose, how to setup needed ones
                var kb = settings.ServiceProvider.GetKeyedService<IKnowledgeBase>(_knowledgeBaseKey);
                kernel.Services.GetKeyedServices<IKnowledgeBase>(_knowledgeBaseKey);
                kernel.Services.GetServices<IKnowledgeBase>();
                context.RetrievedRecords.AddRange(await kb!.SearchAsync(new KnowledgeSearchRequest(), context, cancellationToken));
                //TODO work with knowledge bases and processor
                return recordsToAdd.Count != 0;
            }
            catch (Exception ex)
            {
                return Error.Failure($"{GetType()}: Exception during search execution - {ex.Message}");
            }
        }

        protected override ErrorOr<bool> CheckExecuteConditions(PipelineContext context, bool isRequired = false)
        {
            if (!string.IsNullOrEmpty(context.QueryString))
            {
                return true;
            }

            return FailCondition(isRequired);
        }

        //private DateTimePlugin? GetDateTimePlugin(Microsoft.SemanticKernel.Kernel kernel)
        //{
        //    var plugin = kernel.Plugins.TryGetPlugin<DateTimePlugin>(nameof(DateTimePlugin));
        //    return plugin;
        //}
    }
}
