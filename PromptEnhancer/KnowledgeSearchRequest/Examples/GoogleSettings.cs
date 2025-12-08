using PromptEnhancer.KnowledgeBaseCore.Interfaces;

namespace PromptEnhancer.KnowledgeSearchRequest.Examples
{
    /// <summary>
    /// Represents the configuration settings for integrating with Google's search APIs or scraping services.
    /// </summary>
    /// <remarks>This class provides options for configuring API keys, search engine identifiers, and behavior
    /// for chunking and scraping. It is designed to be used in scenarios where Google's search capabilities are
    /// leveraged for knowledge base searches.</remarks>
    public class GoogleSettings : IKnowledgeBaseSearchSettings
    {
        public required string SearchApiKey { get; set; }
        public required string Engine { get; set; }
        public bool AllowChunking { get; set; } = true;
        public int? ChunkTokenSize { get; set; }
        public int ChunkLimitPerUrl { get; set; } = 5;
        public int TopN { get; set; } = 3;
        public bool UseScraper { get; set; } = false;
    }
}
