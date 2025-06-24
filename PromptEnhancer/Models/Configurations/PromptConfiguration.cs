using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PromptEnhancer.Models.Configurations
{
    public class PromptConfiguration
    {
        [Display(Name = "System Instructions:")]
        public string SystemInstructions { get; set; } = "Create SEO description of product with the help of Used Search Query and Augmented Data";
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
