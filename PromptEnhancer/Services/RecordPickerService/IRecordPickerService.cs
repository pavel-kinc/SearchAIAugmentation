using PromptEnhancer.KnowledgeRecord.Interfaces;
using PromptEnhancer.Models;

namespace PromptEnhancer.Services.RecordPickerService
{
    /// <summary>
    /// Provides functionality to filter and retrieve a collection of knowledge records based on specified criteria.
    /// </summary>
    public interface IRecordPickerService
    {
        /// <summary>
        /// Retrieves a collection of knowledge records that match the specified filter criteria.
        /// </summary>
        /// <remarks>This method applies the specified filter to the provided collection of knowledge
        /// records and returns  only those that meet the filter's conditions. The operation is asynchronous and does
        /// not modify the  original collection of records.</remarks>
        /// <param name="filter">The filter options used to determine which records to include.</param>
        /// <param name="records">The collection of knowledge records to evaluate against the filter.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains an enumerable collection  of
        /// knowledge records that satisfy the filter criteria. If no records match, the result will be an empty
        /// collection.</returns>
        Task<IEnumerable<IKnowledgeRecord>> GetPickedRecordsBasedOnFilter(RecordPickerOptions filter, IEnumerable<IKnowledgeRecord> records);
    }
}
