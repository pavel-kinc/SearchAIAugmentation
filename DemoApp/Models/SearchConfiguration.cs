using PromptEnhancer.KnowledgeBaseCore;

namespace DemoApp.Models
{
    /// <summary>
    /// Represents the configuration settings for a search operation, including provider-specific settings and search
    /// filters.
    /// </summary>
    /// <remarks>This class is currently tailored for Google search configurations. It includes settings for
    /// the search provider  and filters to refine search results.</remarks>
    public class SearchConfiguration
    {
        public SearchProviderSettings SearchProviderSettings { get; set; } = new();

        public GoogleSearchFilterModel SearchFilter { get; set; } = new();
    }
}
