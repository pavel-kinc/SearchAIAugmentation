using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PromptEnhancer.Models.Configurations
{
    public class PromptConfiguration
    {
        public string SystemInstructions { get; set; } = "Create SEO description of product with the help of Used Search Query and Augmented Data";
        public string? MacroDefinition { get; set; }
        public string? AdditionalInstructions { get; set; }
        public int TargetOutputLength { get; set; } = 200;

        public string TargetLanguageCultureCode { get; set; } = "en-US";
    }
}
