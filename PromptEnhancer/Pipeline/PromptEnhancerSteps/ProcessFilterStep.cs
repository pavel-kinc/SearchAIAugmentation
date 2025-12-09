using ErrorOr;
using PromptEnhancer.Models;
using PromptEnhancer.Models.Pipeline;
using PromptEnhancer.Services.RecordPickerService;

namespace PromptEnhancer.Pipeline.PromptEnhancerSteps
{
    /// <summary>
    /// Represents a pipeline step that filters records based on the specified filter criteria.
    /// </summary>
    /// <remarks>This step serves to filter the records according to the provided <see cref="RecordPickerOptions"/>. The filtered  records are
    /// stored in the <see cref="PipelineRun.PickedRecords"/> property for subsequent steps  in the pipeline.</remarks>
    public class ProcessFilterStep : PipelineStep
    {
        private readonly RecordPickerOptions _filter;

        public ProcessFilterStep(RecordPickerOptions filter, bool isRequired = false) : base(isRequired)
        {
            _filter = filter;
        }

        /// <inheritdoc/>
        /// <remarks> This method retrieves the <see cref="IRecordPickerService"/> from the pipeline settings and uses it to filter the records </remarks>
        protected async override Task<ErrorOr<bool>> ExecuteStepAsync(PipelineSettings settings, PipelineRun context, CancellationToken cancellationToken = default)
        {
            var pickerService = settings.GetService<IRecordPickerService>();
            context.PickedRecords = await pickerService!.GetPickedRecordsBasedOnFilter(_filter, context.RetrievedRecords);
            return true;
        }

        /// <inheritdoc/>
        protected override ErrorOr<bool> CheckExecutionConditions(PipelineRun context)
        {
            if (!context.PickedRecords.Any() && context.RetrievedRecords.Any())
            {
                return true;
            }

            return FailCondition();
        }
    }
}
