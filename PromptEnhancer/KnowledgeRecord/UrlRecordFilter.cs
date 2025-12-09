using PromptEnhancer.KnowledgeRecord.Interfaces;
using PromptEnhancer.Models.Examples;

namespace PromptEnhancer.KnowledgeRecord
{
    /// <summary>
    /// Represents a filter for <see cref="UrlRecord"/> objects based on URL content.
    /// </summary>
    /// <remarks>This filter allows you to specify a substring that must be present in the URL of a <see
    /// cref="UrlRecord"/>  for it to be included in the filtered results.</remarks>
    public class UrlRecordFilter : IModelFilter<UrlRecord>
    {
        public string UrlMustContain { get; set; } = "";

        public string ContentMustContain { get; set; } = "";

        public IEnumerable<UrlRecord> FilterModelData(IEnumerable<UrlRecord> records)
        {
            return records.Where(x => x.Url.Contains(UrlMustContain) && x.Content.Contains(ContentMustContain));
        }
    }
}
