using System.ComponentModel.DataAnnotations;

namespace PromptEnhancer.Models.Configurations
{
    public class PromptConfiguration
    {
        [Display(Name = "System Instructions:")]
        public string SystemInstructions { get; set; } = "React to the given prompt: ";//= "Create SEO description of product with the help of Used Search Query and Augmented Data";
        [Display(Name = "Macro definition (optional):")]
        public string? MacroDefinition { get; set; }
        [Display(Name = "Additional Instructions (optional):")]
        public string? AdditionalInstructions { get; set; }
        [Display(Name = "Approximate Output Length:")]
        public int TargetOutputLength { get; set; } = 200;
        [Display(Name = "Output Language:")]
        public string TargetLanguageCultureCode { get; set; } = "en-US";
    }
}
