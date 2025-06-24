using PromptEnhancer.Models;
using System.Globalization;
using System.Linq;

namespace DemoApp.Models
{
    public class DemoAppConfigSetup
    {
        public string AIApiKeyFromInput { get; set; } = string.Empty;
        public string SearchApiKeyFromInput { get; set; } = string.Empty;
        public string SearchEngineFromInput { get; set; } = string.Empty;
        public IEnumerable<string> AvailableCultureCodes { get; set; } = CultureInfo.GetCultures(CultureTypes.SpecificCultures)
                                  .Select(c => c.Name)
                                  .ToList();
    }
}