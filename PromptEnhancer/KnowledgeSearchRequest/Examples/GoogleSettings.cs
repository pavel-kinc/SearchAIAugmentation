using PromptEnhancer.CustomAttributes;
using PromptEnhancer.KnowledgeBase.Interfaces;
using PromptEnhancer.Models;
using System.ComponentModel.DataAnnotations;

namespace PromptEnhancer.KnowledgeSearchRequest.Examples
{
    public class GoogleSettings : IKnowledgeBaseSearchSettings
    {
        public required string SearchApiKey { get; set; }
        public required string Engine { get; set; }
        public required bool AllowChunking { get; set; } = true;
        public int? ChunkSize { get; set; } = 300;
        public int TopN { get; set; } = 3;
        public bool UseScraper { get; set; } = true;
    }
}
