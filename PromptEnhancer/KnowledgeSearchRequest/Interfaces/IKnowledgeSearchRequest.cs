using PromptEnhancer.KnowledgeBaseCore.Interfaces;

namespace PromptEnhancer.KnowledgeSearchRequest.Interfaces
{
    public interface IKnowledgeSearchRequest<TFilter, TSettings> : IKnowledgeSearchRequest
        where TFilter : IKnowledgeBaseSearchFilter
        where TSettings : IKnowledgeBaseSearchSettings
    {
        public TFilter? Filter { get; set; }
        public TSettings Settings { get; set; }
    }

    public interface IKnowledgeSearchRequest
    {

    }
}
