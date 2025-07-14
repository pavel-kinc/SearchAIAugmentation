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
    }
}
