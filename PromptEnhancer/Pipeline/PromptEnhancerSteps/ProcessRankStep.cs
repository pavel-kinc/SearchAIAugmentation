using ErrorOr;
using PromptEnhancer.Models.Pipeline;
using PromptEnhancer.Services.RecordRankerService;

namespace PromptEnhancer.Pipeline.PromptEnhancerSteps
{
    public class ProcessRankStep : PipelineStep
    {
        private readonly string? _recordRankerServiceKey;

        public ProcessRankStep(string? recordRankerServiceKey)
        {
            _recordRankerServiceKey = recordRankerServiceKey;
        }

        protected async override Task<ErrorOr<bool>> ExecuteStepAsync(PipelineSettings settings, PipelineContext context, CancellationToken cancellationToken = default)
        {
            var rankerService = settings.GetService<IRecordRankerService>(_recordRankerServiceKey);
            //TODO is it okay to send context? I would like to make it unmodifiable maybe?
            context.PipelineRankedRecords = await rankerService!.GetEmbeddingsForRecordsWithoutEmbeddingDataAsync(settings.Kernel, context, settings.GeneratorKey);
            return true;
        }

        protected override ErrorOr<bool> CheckExecuteConditions(PipelineContext context)
        {
            if ((context.RetrievedRecords.Any() || context.PipelineEmbeddingsModels.Any()) && !context.PipelineRankedRecords.Any())
            {
                return true;
            }

            return FailCondition();
        }
    }
}
