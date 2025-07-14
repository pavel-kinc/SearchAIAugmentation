using PromptEnhancer.CustomAttributes;
using PromptEnhancer.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace PromptEnhancer.Models
{
    public class SearchProviderData
    {
        [Display(Name = "Search Api Key:")]
        [Sensitive]
        public string? SearchApiKey { get; set; }
        [Display(Name = "Search Engine:")]
        [Sensitive]
        public string? Engine { get; set; }
        [Display(Name = "Search Provider:")]
        public SearchProviderEnum Provider { get; set; }
    }
}
