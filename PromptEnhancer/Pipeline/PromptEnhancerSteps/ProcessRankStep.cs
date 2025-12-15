using ErrorOr;
using PromptEnhancer.Models.Pipeline;
using PromptEnhancer.Services.RecordRankerService;

namespace PromptEnhancer.Pipeline.PromptEnhancerSteps
{
    /// <summary>
    /// Represents a pipeline step that processes and ranks retrieved records based on their similarity to a query.
    /// </summary>
    public class ProcessRankStep : PipelineStep
    {
        public ProcessRankStep(bool isRequired = false) : base(isRequired)
        {
        }

        /// <inheritdoc/>
        /// <remarks>This method uses the <see cref="IRecordRankerService"/> to calculate the similarity
        /// score for the retrieved records based on the provided query string and kernel settings.</remarks>
        protected async override Task<ErrorOr<bool>> ExecuteStepAsync(PipelineSettings settings, PipelineRun context, CancellationToken cancellationToken = default)
        {
            var rankerService = settings.GetService<IRecordRankerService>();
            return await rankerService!.AssignSimilarityScoreToRecordsAsync(settings.Kernel, context.RetrievedRecords, context.QueryString, settings.Settings.GeneratorKey);
        }

        /// <inheritdoc/>
        protected override ErrorOr<bool> CheckExecutionConditions(PipelineRun context)
        {
            if (context.RetrievedRecords.Any())
            {
                return true;
            }

            return FailCondition();
        }
    }
}
