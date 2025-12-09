using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel;
using PromptEnhancer.KnowledgeRecord.Interfaces;

namespace PromptEnhancer.Services.EmbeddingService
{
    /// <summary>
    /// Generates embeddings for the specified knowledge records using the provided kernel.
    /// </summary>
    public interface IEmbeddingService
    {
        /// <summary>
        /// Generates embeddings for a collection of knowledge records asynchronously.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result is <see langword="true"/> if embeddings
        /// were successfully generated; otherwise, <see langword="false"/>.</returns>
        Task<bool> GenerateEmbeddingsForRecordsAsync(Kernel kernel, IReadOnlyList<IKnowledgeRecord> retrievedRecords, string? generatorKey = null, EmbeddingGenerationOptions? options = null, bool skipGenerationForEmbData = false);
    }
}
