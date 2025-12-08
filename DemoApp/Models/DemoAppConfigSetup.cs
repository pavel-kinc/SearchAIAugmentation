using Newtonsoft.Json;
using System.Globalization;

namespace DemoApp.Models
{
    /// <summary>
    /// Represents the configuration settings for the Demo application, including API keys and available culture codes.
    /// </summary>
    /// <remarks>This class provides properties to store API keys and search engine identifiers, as well as a
    /// collection of available culture codes. The <see cref="AvailableCultureCodes"/> property is populated with
    /// specific culture codes based on the current system's culture information.</remarks>
    public class DemoAppConfigSetup
    {
        public string AIApiKeyFromInput { get; set; } = string.Empty;
        public string SearchApiKeyFromInput { get; set; } = string.Empty;
        public string SearchEngineFromInput { get; set; } = string.Empty;
        [JsonIgnore]
        public IEnumerable<string> AvailableCultureCodes { get; set; } = CultureInfo.GetCultures(CultureTypes.SpecificCultures)
                                  .Select(c => c.Name)
                                  .ToList();
    }
}
