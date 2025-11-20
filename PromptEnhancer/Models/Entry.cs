using System.ComponentModel.DataAnnotations;

namespace PromptEnhancer.Models
{
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
