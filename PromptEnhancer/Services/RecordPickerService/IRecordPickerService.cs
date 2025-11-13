using PromptEnhancer.KnowledgeRecord.Interfaces;
using PromptEnhancer.Models;

namespace PromptEnhancer.Services.RecordPickerService
{
    public interface IRecordPickerService
    {
        Task<IEnumerable<IKnowledgeRecord>> GetPickedRecordsBasedOnFilter(RecordPickerOptions filter, IEnumerable<IKnowledgeRecord> records);
    }
}
