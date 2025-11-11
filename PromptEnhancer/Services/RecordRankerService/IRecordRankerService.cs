using Microsoft.SemanticKernel;
using PromptEnhancer.KnowledgeRecord.Interfaces;
using PromptEnhancer.Models.Pipeline;

namespace PromptEnhancer.Services.RecordRankerService
{
    public interface IRecordRankerService
    {
        Task<bool> GetSimilarityScoreForRecordsAsync(Kernel kernel, IEnumerable<IKnowledgeRecord> records, string? queryString, string? generatorKey);
    }
}
