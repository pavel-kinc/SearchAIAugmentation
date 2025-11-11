using PromptEnhancer.KnowledgeRecord.Interfaces;
using PromptEnhancer.Models;
using PromptEnhancer.Models.Pipeline;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PromptEnhancer.Pipeline.PromptEnhancerSteps
{
    public class RecordPickerService : IRecordPickerService
    {
        public virtual async Task<IEnumerable<IKnowledgeRecord>> GetPickedRecordsBasedOnFilter(ProcessRecordPickerOptions filter, PipelineContext context)
        {
            return await GetPickedRecords(context.RetrievedRecords, filter);
        }

        private async Task<IQueryable<IKnowledgeRecord>> GetPickedRecords(IEnumerable<IKnowledgeRecord> value, ProcessRecordPickerOptions filter)
        {
            var query = value.AsQueryable();
            return ApplyPickerOptions(query, filter);
        }

        private IQueryable<IKnowledgeRecord> ApplyPickerOptions(IQueryable<IKnowledgeRecord> query, ProcessRecordPickerOptions filter)
        {
            throw new NotImplementedException();
        }
    }
}
