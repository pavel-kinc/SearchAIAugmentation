using System.ComponentModel.DataAnnotations;

namespace PromptEnhancer.Models
{
    public class Entry
    {
        [Display(Name = "Query for Search:")]
        public string? QueryString { get; set; }
        [Display(Name = "Entry Name:")]
        public string? EntryName { get; set; }
        [Display(Name = "Entry Original Text:")]
        public string? EntryOriginalText { get; set; }
        [Display(Name = "Entry Additional Data:")]
        public string? EntryAdditionalData { get; set; }
    }
}
