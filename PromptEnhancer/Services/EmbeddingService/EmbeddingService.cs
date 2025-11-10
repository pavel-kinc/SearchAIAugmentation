using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel;
using PromptEnhancer.KnowledgeRecord.Interfaces;
using PromptEnhancer.Models;
using System.Collections.Concurrent;

namespace PromptEnhancer.Services.EmbeddingService
{
    public class EmbeddingService : IEmbeddingService
    {
        public virtual async Task<IEnumerable<PipelineEmbeddingsModel>> GetEmbeddingsForRecordsWithoutEmbeddingDataAsync(Kernel kernel, IReadOnlyList<IKnowledgeRecord> retrievedRecords, string? generatorKey = null, EmbeddingGenerationOptions? options = null)
        {
            var generator = kernel.GetRequiredService<IEmbeddingGenerator<string, Embedding<float>>>(generatorKey);
            //TODO maybe override options for overriding given embeddings?
            //TODO now this generates embedding for all records without embeddings data
            var recordsNoEmbedChunks = retrievedRecords.Where(x => !x.HasEmbeddingData).Chunk(200);

            var cb = new ConcurrentBag<PipelineEmbeddingsModel>();
            return await GetEmbeddingModels(options, generator, recordsNoEmbedChunks, cb);
        }

        protected virtual async Task<IEnumerable<PipelineEmbeddingsModel>> GetEmbeddingModels(EmbeddingGenerationOptions? options, IEmbeddingGenerator<string, Embedding<float>> generator, IEnumerable<IKnowledgeRecord[]> recordsNoEmbedChunks, ConcurrentBag<PipelineEmbeddingsModel> cb)
        {
            await Parallel.ForEachAsync(recordsNoEmbedChunks, async (recordsChunk, _) =>
            {
                var embeddings = await generator.GenerateAsync(recordsChunk.Select(r => r.EmbeddingRepresentationString), options, _);

                foreach (var (record, embedding) in recordsChunk.Zip(embeddings))
                {
                    var model = new PipelineEmbeddingsModel
                    {
                        EmbeddingSource = generator.GetType().Name ?? "unknown",
                        EmbeddingModel = embedding.ModelId ?? "unknown",
                        AssociatedRecord = record,
                        EmbeddingVector = embedding.Vector,
                    };
                    cb.Add(model);
                }
            });
            return [.. cb];
        }
    }
}
