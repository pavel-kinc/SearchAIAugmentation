using PromptEnhancer.KnowledgeBase.Interfaces;
using PromptEnhancer.KnowledgeSearchRequest.Interfaces;
using System.Text.Encodings.Web;

namespace PromptEnhancer.KnowledgeSearchRequest
{
    public class KnowledgeSearchRequest<TFilter, TSettings> : IKnowledgeSearchRequest<TFilter, TSettings>
        where TFilter : class, IKnowledgeBaseSearchFilter
        where TSettings : IKnowledgeBaseSearchSettings
    {
        public TFilter? Filter { get; set; }
        public required TSettings Settings { get; set; }
    }
}
