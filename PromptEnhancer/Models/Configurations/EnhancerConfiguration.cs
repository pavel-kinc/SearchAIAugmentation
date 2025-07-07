namespace PromptEnhancer.Models.Configurations
{
    public class EnhancerConfiguration
    {
        public KernelConfiguration? KernelConfiguration { get; set; }

        public SearchConfiguration SearchConfiguration { get; set; } = new();
        public PromptConfiguration PromptConfiguration { get; set; } = new();
    }
}
