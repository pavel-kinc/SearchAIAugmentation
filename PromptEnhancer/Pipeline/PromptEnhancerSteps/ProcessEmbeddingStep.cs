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
            if (context.QueryString is null)
            {
                return false;
            }

            var embService = settings.GetService<IEmbeddingService>(_embeddingServiceKey);
            context.PipelineEmbeddingsModels = await embService!.GetEmbeddingsForRecordsWithoutEmbeddingDataAsync(settings.Kernel, context.RetrievedRecords, settings.GeneratorKey);
            return true;
        }

        protected override ErrorOr<bool> CheckExecuteConditions(PipelineContext context)
        {
            // Need retrieved records and no existing embeddings to avoid redundant processing
            if (context.RetrievedRecords.Count != 0 && !context.PipelineEmbeddingsModels.Any())
            {
                return true;
            }

            return FailCondition();
        }
    }
}
