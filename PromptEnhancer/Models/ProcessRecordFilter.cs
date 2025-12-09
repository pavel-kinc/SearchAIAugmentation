using PromptEnhancer.KnowledgeRecord.Interfaces;

namespace PromptEnhancer.Models
{
    /// <summary>
    /// Represents options for configuring the behavior of a record picker operation.
    /// </summary>
    /// <remarks>This class provides various settings to control how records are filtered, sorted, and
    /// selected during a record picker operation. Use these options to customize the selection criteria,  ordering, and
    /// pagination of the results.</remarks>
    public class RecordPickerOptions
    {
        public double? MinScoreSimilarity { get; init; }

        public int? Take { get; init; }
        public int Skip { get; init; } = 0;

        public string? EmbeddingSourceEquals { get; init; }
        // Custom filter
        public IEnumerable<Func<IKnowledgeRecord, bool>> Predicate { get; init; } = [];
        public IEnumerable<(Func<IKnowledgeRecord, object> KeySelector, bool Descending)> OrderByClauses { get; init; } = [];
        public bool? OrderByScoreDescending { get; init; }
    }
}
