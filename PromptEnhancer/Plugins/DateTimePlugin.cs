using Microsoft.SemanticKernel;
using System.ComponentModel;
using System.Globalization;

namespace PromptEnhancer.Plugins
{
    public class DateTimePlugin
    {
        [KernelFunction("get_current_datetime")]
        [Description("Returns the current datetime in ISO format (YYYY-MM-DD HH:mm:ss).")]
        public string GetCurrentDateTime()
        {
            // DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
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

        [KernelFunction("format_date")]
        [Description("Formats a date according to the given format string.")]
        public string FormatDate(
            [Description("Date in yyyy-MM-dd format.")] string date,
            [Description("Format string (e.g. 'MMMM dd, yyyy')")] string format)
        {
            DateTime parsed = DateTime.ParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            return parsed.ToString(format, CultureInfo.InvariantCulture);
        }
    }
}
