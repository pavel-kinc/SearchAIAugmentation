using System.ComponentModel.DataAnnotations;

namespace PromptEnhancer.Models.Configurations
{
    public class SearchConfiguration
    {
        public SearchProviderData SearchProviderData { get; set; } = new();

        [Display(Name = "Query for Search:")]
        public string? QueryString { get; set; }
    }
}
