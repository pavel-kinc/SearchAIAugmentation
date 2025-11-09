using PromptEnhancer.KnowledgeBase.Interfaces;

namespace PromptEnhancer.KnowledgeBase
{
    public class UrlRecordFilter : IRecordFilter
    {
        public string UrlMustContain { get; set; } = "";
    }
}
