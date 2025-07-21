using PromptEnhancer.Models;

namespace DemoApp.Models
{
    public class EnhancerViewModel
    {
        public ConfigurationSetup ConfigurationSetup { get; set; } = new();
        public IList<ResultModel> ResultModelList { get; set; } = [];

    }
}
