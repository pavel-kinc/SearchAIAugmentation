using PromptEnhancer.KnowledgeBase.Interfaces;

namespace PromptEnhancer.KnowledgeSearchRequest.Examples
{
    public class GoogleSettings : IKnowledgeBaseSearchSettings
    {
        public required string SearchApiKey { get; set; }
        public required string Engine { get; set; }
        public bool AllowChunking { get; set; } = true;
        public int? ChunkSize { get; set; }
        public int ChunkLimitPerUrl { get; set; } = 5;
        public int TopN { get; set; } = 3;
        public bool UseScraper { get; set; } = true;
    }
}
