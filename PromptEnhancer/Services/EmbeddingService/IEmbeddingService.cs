using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel;
using PromptEnhancer.KnowledgeRecord.Interfaces;
using PromptEnhancer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PromptEnhancer.Services.EmbeddingService
{
    public interface IEmbeddingService
    {
        Task<IEnumerable<PipelineEmbeddingsModel>> GetEmbeddingsForRecordsWithoutEmbeddingDataAsync(Kernel kernel, IReadOnlyList<IKnowledgeRecord> retrievedRecords, string? generatorKey = null, EmbeddingGenerationOptions? options = null);
    }
}
