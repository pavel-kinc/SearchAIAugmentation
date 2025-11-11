using PromptEnhancer.KnowledgeRecord.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace PromptEnhancer.Models.Pipeline
{
    public class PipelineRankedRecord
    {
        public required IKnowledgeRecord AssociatedRecord { get; set; }
        [Range(0, 1)]
        public float SimilarityScore { get; init; }
    }
}
