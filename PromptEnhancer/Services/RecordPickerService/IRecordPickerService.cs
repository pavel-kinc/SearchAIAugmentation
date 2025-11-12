using PromptEnhancer.KnowledgeRecord.Interfaces;
using PromptEnhancer.Models;
using PromptEnhancer.Models.Pipeline;

namespace PromptEnhancer.Services.RecordPickerService
{
    internal interface IRecordPickerService
    {
        Task<IEnumerable<IKnowledgeRecord>> GetPickedRecordsBasedOnFilter(RecordPickerOptions filter, IEnumerable<IKnowledgeRecord> records);
    }
}
