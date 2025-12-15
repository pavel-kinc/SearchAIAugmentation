using System.ComponentModel.DataAnnotations;

namespace PromptEnhancer.Models.Configurations
{
    /// <summary>
    /// Represents the configuration settings for generating a prompt, including system instructions, optional macros,
    /// and output preferences.
    /// </summary>
    /// <remarks>It includes settings for system instructions, optional macro definitions, additional instructions, 
    /// the desired output length, and the target language for the output.</remarks>
    public class PromptConfiguration
    {
        [Display(Name = "System Instructions:")]
        public string SystemInstructions { get; set; } = "React to the given prompt: ";
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
