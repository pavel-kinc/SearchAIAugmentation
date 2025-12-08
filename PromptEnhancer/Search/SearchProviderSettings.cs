using PromptEnhancer.CustomAttributes;
using PromptEnhancer.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace PromptEnhancer.Search
{
    /// <summary>
    /// Represents the configuration settings for a search provider, including API key, engine, and provider type.
    /// </summary>
    /// <remarks>This class is used to configure the necessary parameters for interacting with a search
    /// provider.  Sensitive properties, such as the API key, should be handled securely to prevent unauthorized
    /// access.</remarks>
    public class SearchProviderSettings
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
