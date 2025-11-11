using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel;
using PromptEnhancer.KnowledgeRecord.Interfaces;
using PromptEnhancer.Models;

namespace PromptEnhancer.Services.EmbeddingService
{
    public interface IEmbeddingService
    {
        Task<bool> GetEmbeddingsForRecordsWithoutEmbeddingDataAsync(Kernel kernel, IReadOnlyList<IKnowledgeRecord> retrievedRecords, string? generatorKey = null, EmbeddingGenerationOptions? options = null);
    }
}
