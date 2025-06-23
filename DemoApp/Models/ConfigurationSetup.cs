using Microsoft.AspNetCore.Mvc;
using PromptEnhancer.Models.Configurations;

namespace DemoApp.Models
{
    public class ConfigurationSetup
    {
        public KernelConfiguration KernelConfiguration { get; set; } = new();
        public SearchConfiguration SearchConfiguration { get; set; } = new();
        public PromptConfiguration PromptConfiguration { get; set; } = new();
        public DemoAppConfigSetup DemoAppConfigSetup { get; set; } = new();
    }
}
