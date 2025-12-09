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
        /// <param name="query">The initial collection of <see cref="IKnowledgeRecord"/> to which the options will be applied.</param>
        /// <param name="options">The <see cref="RecordPickerOptions"/> specifying the filtering, ordering, and paging criteria.</param>
        /// <returns>A filtered, ordered, and paged collection of <see cref="IKnowledgeRecord"/> based on the specified options.</returns>
        private IEnumerable<IKnowledgeRecord> ApplyPickerOptions(IEnumerable<IKnowledgeRecord> query, RecordPickerOptions options)
        {
            query = options.MinScoreSimilarity.HasValue ? query.Where(r => r.SimilarityScore.HasValue && r.SimilarityScore.Value >= options.MinScoreSimilarity.Value) : query;

            query = !string.IsNullOrWhiteSpace(options.EmbeddingSourceEquals) ?
                query.Where(r => r.Embeddings != null && r.Embeddings.EmbeddingSource == options.EmbeddingSourceEquals) : query;

            foreach (var predicate in options.Predicate)
            {
                query = query.Where(predicate);
            }

            foreach (var (keySelector, descending) in options.OrderByClauses)
            {
                query = descending
                    ? query.OrderByDescending(keySelector)
                    : query.OrderBy(keySelector);
            }

            query = options.OrderByScoreDescending.HasValue && options.OrderByScoreDescending.Value ? query.OrderByDescending(x => x.SimilarityScore) : query;

            // Paging
            if (options.Skip > 0)
                query = query.Skip(options.Skip);

            if (options.Take.HasValue)
                query = query.Take(options.Take.Value);

            return query;
        }
    }
}
