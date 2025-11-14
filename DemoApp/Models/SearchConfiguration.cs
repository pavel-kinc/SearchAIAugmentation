using PromptEnhancer.KnowledgeBase;

namespace DemoApp.Models
{
    //TODO for now only for google (should i keep things that point to multiple options?)
    public class SearchConfiguration
    {
        public SearchProviderSettings SearchProviderSettings { get; set; } = new();

        public GoogleSearchFilterModel SearchFilter { get; set; } = new();
    }
}
