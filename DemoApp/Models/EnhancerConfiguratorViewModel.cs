using PromptEnhancer.Models;

namespace DemoApp.Models
{
    public class EnhancerViewModel
    {
        public ConfigurationSetup ConfigurationSetup { get; set; } = new();
        public ResultModel? ResultModel { get; set; }

    }
}
