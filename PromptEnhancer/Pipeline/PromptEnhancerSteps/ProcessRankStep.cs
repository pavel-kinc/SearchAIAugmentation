using ErrorOr;
using PromptEnhancer.Models.Pipeline;
using PromptEnhancer.Services.RecordRankerService;

namespace PromptEnhancer.Pipeline.PromptEnhancerSteps
{
    public class ProcessRankStep : PipelineStep
    {
        private readonly string? _recordRankerServiceKey;

        public ProcessRankStep(string? recordRankerServiceKey, bool isRequired = false) : base(isRequired)
        {
            _recordRankerServiceKey = recordRankerServiceKey;
        }

        protected async override Task<ErrorOr<bool>> ExecuteStepAsync(PipelineSettings settings, PipelineContext context, CancellationToken cancellationToken = default)
        {
            var rankerService = settings.GetService<IRecordRankerService>(_recordRankerServiceKey);
            //TODO is it okay to send context? I would like to make it unmodifiable maybe?
            return await rankerService!.GetSimilarityScoreForRecordsAsync(settings.Kernel, context.RetrievedRecords, context.QueryString, settings.GeneratorKey);
        }

        protected override ErrorOr<bool> CheckExecuteConditions(PipelineContext context)
        {
            if (context.RetrievedRecords.Any())
            {
                return true;
            }

            return FailCondition();
        }
    }
}
