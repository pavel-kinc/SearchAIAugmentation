using Microsoft.SemanticKernel;
using PromptEnhancer.Plugins.Interfaces;
using System.ComponentModel;

namespace PromptEnhancer.Plugins
{
    /// <summary>
    /// Provides functionality to retrieve the current date and time in a specific format.
    /// </summary>
    /// <remarks>This plugin is designed to be used within a semantic kernel context to provide the current
    /// date and time as a string formatted as "YYYY-MM-dd HH:mm:ss".</remarks>
    public class DateTimePlugin : ISemanticKernelContextPlugin
    {
        [KernelFunction("get_context_current_datetime")]
        [Description("Returns the current datetime (YYYY-MM-dd HH:mm:ss) as context.")]
        public string GetCurrentDateTime()
        {
            return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }
    }
}
