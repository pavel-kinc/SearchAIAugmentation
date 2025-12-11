using Microsoft.Extensions.Logging;
using PromptEnhancer.KnowledgeRecord.Interfaces;
using PromptEnhancer.Models;

namespace PromptEnhancer.Services.RecordPickerService
{
    /// <summary>
    /// Provides functionality to filter, order, and page collections of knowledge records based on specified criteria.
    /// </summary>
    /// <remarks>This service is designed to process collections of <see cref="IKnowledgeRecord"/> by applying
    /// filtering, ordering,  and paging options defined in <see cref="RecordPickerOptions"/>. It is intended for
    /// scenarios where dynamic  selection of records is required based on customizable criteria.</remarks>
    public class RecordPickerService : IRecordPickerService
    {
        private readonly ILogger<RecordPickerService> _logger;

        public RecordPickerService(ILogger<RecordPickerService> logger)
        {
            _logger = logger;
        }
        /// <inheritdoc/>
        public virtual async Task<IEnumerable<IKnowledgeRecord>> GetPickedRecordsBasedOnFilter(RecordPickerOptions filter, IEnumerable<IKnowledgeRecord> records)
        {
            return ApplyPickerOptions(records, filter);
        }

        /// <summary>
        /// Applies filtering, ordering, and paging options to a collection of knowledge records.
        /// </summary>
        /// <remarks>This method applies the following transformations to the input collection: <list
        /// type="bullet"> <item> <description>Filters records based on similarity score, embedding source, and custom
        /// predicates.</description> </item> <item> <description>Orders records based on specified key selectors and
        /// optional descending order.</description> </item> <item> <description>Applies paging by skipping a specified
        /// number of records and taking a limited number of records.</description> </item> </list> If no options are
        /// specified, the method returns the input collection unchanged.</remarks>
        /// <param name="records">The initial collection of <see cref="IKnowledgeRecord"/> to which the options will be applied.</param>
        /// <param name="options">The <see cref="RecordPickerOptions"/> specifying the filtering, ordering, and paging criteria.</param>
        /// <returns>A filtered, ordered, and paged collection of <see cref="IKnowledgeRecord"/> based on the specified options.</returns>
        private IEnumerable<IKnowledgeRecord> ApplyPickerOptions(IEnumerable<IKnowledgeRecord> records, RecordPickerOptions options)
        {
            _logger.LogDebug("Applying RecordPickerOptions to {RecordCount} records.", records.Count());
            records = options.MinScoreSimilarity.HasValue ? records.Where(r => r.SimilarityScore.HasValue && r.SimilarityScore.Value >= options.MinScoreSimilarity.Value) : records;

            records = !string.IsNullOrWhiteSpace(options.EmbeddingSourceEquals) ?
                records.Where(r => r.Embeddings != null && r.Embeddings.EmbeddingSource == options.EmbeddingSourceEquals) : records;

            foreach (var predicate in options.Predicate)
            {
                records = records.Where(predicate);
            }

            foreach (var (keySelector, descending) in options.OrderByClauses)
            {
                records = descending
                    ? records.OrderByDescending(keySelector)
                    : records.OrderBy(keySelector);
            }

            records = options.OrderByScoreDescending.HasValue && options.OrderByScoreDescending.Value ? records.OrderByDescending(x => x.SimilarityScore) : records;

            // Paging
            if (options.Skip > 0)
                records = records.Skip(options.Skip);

            if (options.Take.HasValue)
                records = records.Take(options.Take.Value);

            return records;
        }
    }
}
