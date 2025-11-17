using ErrorOr;
using Microsoft.Extensions.AI;
using PromptEnhancer.Models.Pipeline;
using PromptEnhancer.Services.EmbeddingService;

namespace PromptEnhancer.Pipeline.PromptEnhancerSteps
{
    public class ProcessEmbeddingStep : PipelineStep
    {
        private readonly string? _embeddingServiceKey;
        private readonly bool _skipGenerationForEmbData;
        private readonly EmbeddingGenerationOptions? _options;

        public ProcessEmbeddingStep(string? embeddingServiceKey = null, bool skipGenerationForEmbData = false, EmbeddingGenerationOptions? options = null, string? generatorKey = null, bool isRequired = false) : base(isRequired)
        {
            _embeddingServiceKey = embeddingServiceKey;
            _skipGenerationForEmbData = skipGenerationForEmbData;
            _options = options;
        }

        protected override async Task<ErrorOr<bool>> ExecuteStepAsync(PipelineSettings settings, PipelineContext context, CancellationToken cancellationToken = default)
        {
            var embService = settings.GetService<IEmbeddingService>(_embeddingServiceKey);
            return await embService!.GenerateEmbeddingsForRecordsAsync(settings.Kernel, context.RetrievedRecords, settings.Settings.GeneratorKey, _options, _skipGenerationForEmbData);
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
