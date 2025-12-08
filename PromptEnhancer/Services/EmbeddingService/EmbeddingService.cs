using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel;
using PromptEnhancer.KnowledgeRecord.Interfaces;
using PromptEnhancer.Models;

namespace PromptEnhancer.Services.EmbeddingService
{
    /// <summary>
    /// Provides functionality for generating embeddings for a collection of knowledge records.
    /// </summary>
    /// <remarks>This service is designed to process knowledge records and generate embeddings using a
    /// specified embedding generator. It supports chunking of records to handle large datasets efficiently and allows
    /// for optional skipping of records that already contain embedding data. The service relies on an <see
    /// cref="IEmbeddingGenerator{TInput, TOutput}"/>  implementation to perform the embedding generation.</remarks>
    public class EmbeddingService : IEmbeddingService
    {
        public virtual async Task<bool> GenerateEmbeddingsForRecordsAsync(Kernel kernel, IReadOnlyList<IKnowledgeRecord> retrievedRecords, string? generatorKey = null, EmbeddingGenerationOptions? options = null, bool skipGenerationForEmbData = false)
        {
            var generator = kernel.GetRequiredService<IEmbeddingGenerator<string, Embedding<float>>>(generatorKey);
            var recordsNoEmbed = retrievedRecords.Where(x => (!skipGenerationForEmbData ? x.Embeddings is null : !x.HasEmbeddingData) && !string.IsNullOrWhiteSpace(x.EmbeddingRepresentationString));
            var recordsChunked = recordsNoEmbed.Chunk(200);

            return await AssignEmbeddingModels(options, generator, recordsChunked);
        }

        /// <summary>
        /// Assigns embedding models to a collection of knowledge records using the specified embedding generator.
        /// </summary>
        /// <remarks>This method processes the provided knowledge records in parallel, generating
        /// embeddings for each record using the specified generator. The generated embeddings are assigned to the
        /// corresponding records, including metadata such as the embedding source and model ID.</remarks>
        /// <param name="options">The options to configure the embedding generation process. Can be <see langword="null"/> to use default
        /// options.</param>
        /// <param name="generator">The embedding generator used to create embeddings for the knowledge records.</param>
        /// <param name="recordsNoEmbedChunks">A collection of chunks, where each chunk contains knowledge records without embeddings.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is <see langword="true"/> when the
        /// operation completes successfully.</returns>
        protected virtual async Task<bool> AssignEmbeddingModels(EmbeddingGenerationOptions? options, IEmbeddingGenerator<string, Embedding<float>> generator, IEnumerable<IKnowledgeRecord[]> recordsNoEmbedChunks)
        {
            await Parallel.ForEachAsync(recordsNoEmbedChunks, async (recordsChunk, _) =>
            {
                var embeddings = await generator.GenerateAsync(recordsChunk.Select(r => r.EmbeddingRepresentationString), options, _);

                foreach (var (record, embedding) in recordsChunk.Zip(embeddings))
                {
                    record.Embeddings = new PipelineEmbeddingsModel
                    {
                        EmbeddingSource = generator.GetType().Name ?? "unknown",
                        EmbeddingModel = embedding.ModelId,
                        EmbeddingVector = embedding.Vector
                    };
                }
            });
            return true;
        }
    }
}
