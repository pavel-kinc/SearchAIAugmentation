using Microsoft.SemanticKernel;
using PromptEnhancer.ChunkUtilities.Interfaces;
using PromptEnhancer.Plugins.Interfaces;
using System.ComponentModel;
using System.Globalization;

namespace PromptEnhancer.Plugins
{
    public class DateTimePlugin : ISemanticKernelPlugin
    {
        private readonly IChunkService _kernelManager;

        public DateTimePlugin(IChunkService kernelManager)
        {
            _kernelManager = kernelManager;
        }

        [KernelFunction("get_current_datetime")]
        [Description("Returns the current datetime (YYYY-MM-dd HH:mm:ss).")]
        public string GetCurrentDateTime()
        {
            return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }

        [KernelFunction("add_days")]
        [Description("Adds a number of days to a given date and returns the new date.")]
        public string AddDays(
            [Description("Base date in yyyy-MM-dd format.")] string date,
            [Description("Number of days to add (can be negative).")] int days)
        {
            DateTime parsed = DateTime.ParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            return parsed.AddDays(days).ToString("yyyy-MM-dd");
        }
    }
}
