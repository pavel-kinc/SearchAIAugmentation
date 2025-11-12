using PromptEnhancer.KnowledgeRecord.Interfaces;

namespace PromptEnhancer.Models
{
    public class RecordPickerOptions
    {
        public float? MinScoreSimilarity { get; init; }

        public int? Take { get; init; }
        public int Skip { get; init; } = 0;

        public string? EmbeddingSourceEquals { get; init; }
        // Custom filter
        public IEnumerable<Func<IKnowledgeRecord, bool>> Predicate { get; init; } = [];
        public IEnumerable<(Func<IKnowledgeRecord, object> KeySelector, bool Descending)> OrderByClauses { get; init; } = [];
        public bool? OrderByScoreDescending { get; init; }
    }
}
