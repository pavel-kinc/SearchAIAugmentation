using ErrorOr;
using PromptEnhancer.Models.Pipeline;
using PromptEnhancer.Services.EmbeddingService;

namespace PromptEnhancer.Pipeline.PromptEnhancerSteps
{
    public class ProcessEmbeddingStep : PipelineStep
    {
        private readonly string? _embeddingServiceKey;

        public ProcessEmbeddingStep(string? embeddingServiceKey = null, bool isRequired = false, string? generatorKey = null)
        {
            _isRequired = isRequired;
            _embeddingServiceKey = embeddingServiceKey;
        }

        protected override async Task<ErrorOr<bool>> ExecuteStepAsync(PipelineSettings settings, PipelineContext context, CancellationToken cancellationToken = default)
        {
            var embService = settings.GetService<IEmbeddingService>(_embeddingServiceKey);
            return await embService!.GetEmbeddingsForRecordsWithoutEmbeddingDataAsync(settings.Kernel, context.RetrievedRecords, settings.GeneratorKey);
        }

        protected override ErrorOr<bool> CheckExecuteConditions(PipelineContext context)
        {
            // Need retrieved records and no existing embeddings to avoid redundant processing
            if (context.RetrievedRecords.Any())
            {
                return true;
            }

            return FailCondition();
        }
    }
}
