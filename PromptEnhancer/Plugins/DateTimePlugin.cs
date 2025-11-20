using Microsoft.SemanticKernel;
using PromptEnhancer.Plugins.Interfaces;
using System.ComponentModel;

namespace PromptEnhancer.Plugins
{
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
