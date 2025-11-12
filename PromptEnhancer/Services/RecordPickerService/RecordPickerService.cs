using PromptEnhancer.KnowledgeRecord.Interfaces;
using PromptEnhancer.Models;

namespace PromptEnhancer.Services.RecordPickerService
{
    public class RecordPickerService : IRecordPickerService
    {
        public virtual async Task<IEnumerable<IKnowledgeRecord>> GetPickedRecordsBasedOnFilter(RecordPickerOptions filter, IEnumerable<IKnowledgeRecord> records)
        {
            return ApplyPickerOptions(records, filter);
        }

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
