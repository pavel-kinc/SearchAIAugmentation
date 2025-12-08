using System.ComponentModel.DataAnnotations;

namespace PromptEnhancer.Models
{
    /// <summary>
    /// Represents an user entry containing information such as a query string, name, original text, and additional data.
    /// You can also straight-up use the <see cref="PipelineRun"/>.
    /// </summary>
    /// <remarks>This class is designed to encapsulate user input related to an entry, including metadata and
    /// descriptive fields. Each property is immutable after initialization, ensuring the integrity of the entry's
    /// data.</remarks>
    public class Entry
    {
        [Display(Name = "Query for Search:")]
        public string? QueryString { get; init; }
        [Display(Name = "Entry Name:")]
        public string? EntryName { get; init; }
        [Display(Name = "Entry Original Text:")]
        public string? EntryOriginalText { get; init; }
        [Display(Name = "Entry Additional Data:")]
        public string? EntryAdditionalData { get; init; }
    }
}
