using PromptEnhancer.KnowledgeRecord.Interfaces;

namespace PromptEnhancer.KnowledgeBaseCore.Interfaces
{
    public interface IKnowledgeBaseContainer
    {
        public string Description { get; }
        public Task<IEnumerable<IKnowledgeRecord>> SearchAsync(IEnumerable<string> queriesToSearch, CancellationToken ct = default);
    }
}
