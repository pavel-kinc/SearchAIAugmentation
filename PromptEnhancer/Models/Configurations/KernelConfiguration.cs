using PromptEnhancer.CustomAttributes;
using PromptEnhancer.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace PromptEnhancer.Models.Configurations
{
    /// <summary>
    /// Represents the configuration settings for an AI kernel, including model selection, API keys, and provider
    /// details.
    /// </summary>
    /// <remarks>This class is used to configure the behavior of an AI kernel by specifying the model, API
    /// keys, and other related settings. Sensitive properties, such as API keys, should be handled securely and not
    /// exposed in logs or user interfaces.</remarks>
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
        /// <summary>
        /// Gets or sets a value indicating whether the configuration for embeddings should be derived from the LLM
        /// configuration. Important in creating AI services.
        /// </summary>
        public bool UseLLMConfigForEmbeddings { get; set; } = false;
    }
}
