using Microsoft.SemanticKernel;
using PromptEnhancer.Models.Pipeline;

namespace PromptEnhancer.Services.RecordRankerService
{
    public interface IRecordRankerService
    {
        Task<IEnumerable<PipelineRankedRecord>> GetEmbeddingsForRecordsWithoutEmbeddingDataAsync(Kernel kernel, PipelineContext context, string? generatorKey);
    }
}
