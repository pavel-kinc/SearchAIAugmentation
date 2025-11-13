using Microsoft.SemanticKernel;
using PromptEnhancer.Plugins.Interfaces;
using System.ComponentModel;

namespace PromptEnhancer.Plugins
{
    public class TemperaturePlugin : SemanticKernelPlugin, ISemanticKernelContextPlugin
    {
        [KernelFunction("get_context_current_temperature")]
        [Description("Returns the current temperature as context.")]
        public string GetContextCurrentTemperature()
        {
            return "29 degrees celsium";
        }
    }
}
