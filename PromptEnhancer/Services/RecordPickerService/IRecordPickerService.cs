using PromptEnhancer.KnowledgeRecord.Interfaces;
using PromptEnhancer.Models.Pipeline;

namespace PromptEnhancer.Pipeline.PromptEnhancerSteps
{
    internal interface IRecordPickerService
    {
        Task<IEnumerable<IKnowledgeRecord>> GetPickedRecordsBasedOnFilter(RecordPickerOptions filter, IEnumerable<IKnowledgeRecord> records);
    }
}
