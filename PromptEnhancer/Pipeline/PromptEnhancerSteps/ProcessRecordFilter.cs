using AngleSharp.Dom;
using PromptEnhancer.KnowledgeRecord.Interfaces;
using System;
using System.Globalization;

namespace PromptEnhancer.Pipeline.PromptEnhancerSteps
{
    public class ProcessRecordPickerOptions
    {
        public float? MinScoreSimilarity { get; init; }     // applies to record.RankSimilarity

        public int? Take { get; init; }
        public int Skip { get; init; } = 0;

        public string? EmbeddingSourceEquals { get; init; }
        // Custom filter
        public IEnumerable<Func<IKnowledgeRecord, bool>> Predicate { get; init; } = [];
        public IEnumerable<(Func<IKnowledgeRecord, object> KeySelector, bool Descending)> OrderByClauses { get; init; } = [];
        public bool OrderByScoreDescending { get; init; } = true;
    }
}
