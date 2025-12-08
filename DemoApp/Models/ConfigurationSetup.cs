using PromptEnhancer.Models.Configurations;

namespace DemoApp.Models
{
    /// <summary>
    /// Represents the configuration settings for various components of the application.
    /// </summary>
    /// <remarks>This class provides a centralized structure for managing configuration settings  related to
    /// the kernel, search, prompts, generation, and demo application setup.  Each property corresponds to a specific
    /// configuration category, allowing for  modular and organized configuration management.</remarks>
    public class ConfigurationSetup
    {
        public KernelConfiguration KernelConfiguration { get; set; } = new();
        public SearchConfiguration SearchConfiguration { get; set; } = new();
        public PromptConfiguration PromptConfiguration { get; set; } = new();
        public GenerationConfiguration GenerationConfiguration { get; set; } = new();
        public DemoAppConfigSetup DemoAppConfigSetup { get; set; } = new();
    }
}
