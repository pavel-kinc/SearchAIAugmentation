using PromptEnhancer.KnowledgeRecord.Interfaces;
using PromptEnhancer.Models.Examples;

namespace PromptEnhancer.KnowledgeRecord
{
    public class UrlRecordFilter : IRecordFilter<UrlRecord>
    {
        public string UrlMustContain { get; set; } = "";

        public IEnumerable<UrlRecord> FilterRecords(IEnumerable<UrlRecord> records)
        {
            return records.Where(x => x.Url.Contains(UrlMustContain));
        }
    }
}
