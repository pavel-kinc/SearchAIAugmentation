using ErrorOr;
using PromptEnhancer.Models;
using PromptEnhancer.Models.Pipeline;
using PromptEnhancer.Services.EmbeddingService;
using PromptEnhancer.Services.RecordPickerService;

namespace PromptEnhancer.Pipeline.PromptEnhancerSteps
{
    public class ProcessFilterStep : PipelineStep
    {
        private readonly RecordPickerOptions _filter;
        private readonly string? _pickerServiceKey;

        public ProcessFilterStep(RecordPickerOptions filter, string? pickerServiceKey = null, bool isRequired = false) : base(isRequired)
        {
            _filter = filter;
            _pickerServiceKey = pickerServiceKey;
        }
        protected async override Task<ErrorOr<bool>> ExecuteStepAsync(PipelineSettings settings, PipelineContext context, CancellationToken cancellationToken = default)
        {
            var pickerService = settings.GetService<IRecordPickerService>(_pickerServiceKey);
            context.PickedRecords = await pickerService!.GetPickedRecordsBasedOnFilter(_filter, context.RetrievedRecords);
            return true;
        }

        protected override ErrorOr<bool> CheckExecuteConditions(PipelineContext context)
        {
            if (!context.PickedRecords.Any() && context.RetrievedRecords.Any())
            {
                return true;
            }

            return FailCondition();
        }
    }
}
