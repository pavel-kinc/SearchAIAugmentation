using ErrorOr;
using Microsoft.Extensions.AI;
using PromptEnhancer.Models.Pipeline;
using PromptEnhancer.Services.EmbeddingService;

namespace PromptEnhancer.Pipeline.PromptEnhancerSteps
{
    /// <summary>
    /// Represents a pipeline step that processes embeddings for retrieved records.
    /// </summary>
    /// <remarks>This step generates embeddings for the retrieved records using the configured embedding
    /// service.  The behavior of the step can be customized using the provided options and the flag to skip generation 
    /// for embedding data. The step ensures that retrieved records are available before execution.</remarks>
    public class ProcessEmbeddingStep : PipelineStep
    {
        private readonly bool _skipGenerationForEmbData;
        private readonly EmbeddingGenerationOptions? _options;

        public ProcessEmbeddingStep(bool skipGenerationForEmbData = false, EmbeddingGenerationOptions? options = null, bool isRequired = false) : base(isRequired)
        {
            _skipGenerationForEmbData = skipGenerationForEmbData;
            _options = options;
        }

        /// <inheritdoc/>
        /// <remarks>This method retrieves the embedding service from the pipeline settings and invokes
        /// the embedding generation  process for the specified records. Ensure that the pipeline settings include a
        /// valid embedding service  and generator key.</remarks>
        protected override async Task<ErrorOr<bool>> ExecuteStepAsync(PipelineSettings settings, PipelineRun context, CancellationToken cancellationToken = default)
        {
            var embService = settings.GetService<IEmbeddingService>();
            return await embService!.GenerateEmbeddingsForRecordsAsync(settings.Kernel, context.RetrievedRecords, settings.Settings.GeneratorKey, _options, _skipGenerationForEmbData);
        }

        /// <inheritdoc/>
        protected override ErrorOr<bool> CheckExecutionConditions(PipelineRun context)
        {
            // Need retrieved records
            if (context.RetrievedRecords.Any())
            {
                return true;
            }

            return FailCondition();
        }
    }
}
