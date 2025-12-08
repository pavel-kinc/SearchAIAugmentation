using PromptEnhancer.Models;

namespace DemoApp.Models
{
    public class EnhancerViewModel
    {
        public ConfigurationSetup ConfigurationSetup { get; set; } = new();
        public List<string> Errors { get; set; } = [];
        public IList<PipelineResultModel> ResultModelList { get; set; } = [];
        public string? FloatingAlertMessage { get; set; }
    }
}
