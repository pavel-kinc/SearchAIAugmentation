using System.ComponentModel.DataAnnotations;

namespace PromptEnhancer.Models.Configurations
{
    public class SearchConfiguration
    {
        public SearchProviderData SearchProviderData { get; set; } = new();
    }
}
