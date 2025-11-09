using PromptEnhancer.KnowledgeBase.Interfaces;

namespace PromptEnhancer.KnowledgeSearchRequest.Interfaces
{
    public interface IKnowledgeSearchRequest<TFilter, TSettings> 
        where TFilter : IKnowledgeBaseSearchFilter
        where TSettings : IKnowledgeBaseSearchSettings
    {
        public TFilter? Filter { get; set; }
        public TSettings Settings { get; set; }
    }
}
