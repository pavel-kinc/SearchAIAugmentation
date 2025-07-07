using PromptEnhancer.Models;

namespace DemoApp.Models
{
    public class EnhancerConfiguratorViewModel
    {
        public ConfigurationSetup ConfigurationSetup { get; set; } = new();
        public ResultModel? ResultModel { get; set; }

    }
}
