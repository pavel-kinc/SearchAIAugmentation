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
            //TODO maybe key cant be null? but there is no keyed service so mb okay
            var generator = kernel.GetRequiredService<IEmbeddingGenerator<string, Embedding<float>>>(generatorKey);
            //TODO now this generates embedding for all records without embeddings data
            //TODO maybe add settings for embedding limitations, like max number of records or max size of record/records
            var recordsNoEmbed = retrievedRecords.Where(x => !x.HasEmbeddingData);
            var recordsChunked = recordsNoEmbed.Chunk(200);

            return await GetEmbeddingModels(options, generator, recordsChunked);
        }

        protected virtual async Task<IEnumerable<PipelineEmbeddingsModel>> GetEmbeddingModels(EmbeddingGenerationOptions? options, IEmbeddingGenerator<string, Embedding<float>> generator, IEnumerable<IKnowledgeRecord[]> recordsNoEmbedChunks)
        {
            var cb = new ConcurrentBag<PipelineEmbeddingsModel>();
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
