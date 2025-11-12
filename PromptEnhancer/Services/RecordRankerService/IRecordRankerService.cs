using Microsoft.SemanticKernel;
using PromptEnhancer.KnowledgeRecord.Interfaces;

namespace PromptEnhancer.Services.RecordRankerService
{
    public interface IRecordRankerService
    {
        Task<bool> GetSimilarityScoreForRecordsAsync(Kernel kernel, IEnumerable<IKnowledgeRecord> records, string? queryString, string? generatorKey);
    }
}
