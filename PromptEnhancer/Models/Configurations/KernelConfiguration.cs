using PromptEnhancer.CustomAttributes;
using PromptEnhancer.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace PromptEnhancer.Models.Configurations
{
    public class KernelConfiguration
    {
        [Display(Name = "AI Model:")]
        public string? Model { get; set; }
        [Display(Name = "AI Api Key:")]
        [Sensitive]
        public string? AIApiKey { get; set; }
        [Display(Name = "AI Provider:")]
        public AIProviderEnum Provider { get; set; }
        public string? DeploymentName { get; set; }
        public string? ClientServiceId { get; set; }
        [Display(Name = "Embedding Model:")]
        public string? EmbeddingModel { get; set; }
        [Display(Name = "Embedding Key:")]
        [Sensitive]
        public string? EmbeddingKey { get; set; }
        [Display(Name = "Embedding Provider:")]
        public AIProviderEnum? EmbeddingProvider { get; set; }
        public string? GeneratorServiceId { get; set; }
        public bool UseLLMConfigForEmbeddings { get; set; } = false;
    }
}
