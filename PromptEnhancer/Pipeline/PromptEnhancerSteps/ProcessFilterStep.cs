using ErrorOr;
using PromptEnhancer.Models.Pipeline;
using PromptEnhancer.Services.EmbeddingService;

namespace PromptEnhancer.Pipeline.PromptEnhancerSteps
{
    public class ProcessFilterStep : PipelineStep
    {
        private readonly ProcessRecordPickerOptions _filter;
        private readonly string? _pickerServiceKey;

        public ProcessFilterStep(ProcessRecordPickerOptions filter, string? pickerServiceKey = null, bool isRequired = false) : base(isRequired)
        {
            _filter = filter;
            _pickerServiceKey = pickerServiceKey;
        }
        protected async override Task<ErrorOr<bool>> ExecuteStepAsync(PipelineSettings settings, PipelineContext context, CancellationToken cancellationToken = default)
        {
            var pickerService = settings.GetService<IRecordPickerService>(_pickerServiceKey);
            context.PickedRecords = await pickerService!.GetPickedRecordsBasedOnFilter(_filter, context);
            return true;
        }

        protected override ErrorOr<bool> CheckExecuteConditions(PipelineContext context)
        {
            if (!context.PickedRecords.Any() && (context.RetrievedRecords.Any() || context.PipelineRankedRecords.Any() || context.PipelineEmbeddingsModels.Any()))
            {
                return true;
            }

            return FailCondition();
        }
    }
}
