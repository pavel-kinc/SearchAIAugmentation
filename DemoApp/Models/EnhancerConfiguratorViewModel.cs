using PromptEnhancer.Models;

namespace DemoApp.Models
{
    /// <summary>
    /// Represents the view model for the enhancer, containing configuration, results, errors, and alert messages.
    /// </summary>
    /// <remarks>This class is designed to encapsulate the state and data required for the enhancer's
    /// operation,  including configuration settings, processing results, error messages, and optional floating
    /// alerts.</remarks>
    public class EnhancerViewModel
    {
        public ConfigurationSetup ConfigurationSetup { get; set; } = new();
        public List<string> Errors { get; set; } = [];
        public IList<PipelineResultModel> ResultModelList { get; set; } = [];
        public string? FloatingAlertMessage { get; set; }
    }
}
