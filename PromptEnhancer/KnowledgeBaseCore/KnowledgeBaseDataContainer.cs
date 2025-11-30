using PromptEnhancer.KnowledgeBaseCore.Interfaces;
using PromptEnhancer.KnowledgeRecord;
using PromptEnhancer.KnowledgeRecord.Interfaces;

namespace PromptEnhancer.KnowledgeBaseCore
{
    public class KnowledgeBaseDataContainer<TRecord, TModel> : IKnowledgeBaseContainer
        where TRecord : KnowledgeRecord<TModel>, new()
        where TModel : class
    {
        private readonly KnowledgeBaseDefault<TRecord, TModel> _knowledgeBase;
        private readonly IEnumerable<TModel> _data;

        public KnowledgeBaseDataContainer(KnowledgeBaseDefault<TRecord, TModel> knowledgeBaseDefault, IEnumerable<TModel> data)
        {
            _knowledgeBase = knowledgeBaseDefault;
            _data = data;
        }

        public virtual string Description => _knowledgeBase.Description;

        public async virtual Task<IEnumerable<IKnowledgeRecord>> SearchAsync(IEnumerable<string> queriesToSearch, CancellationToken ct = default)
        {
            return _knowledgeBase.GetRecords(_data, queriesToSearch.FirstOrDefault(x => !string.IsNullOrEmpty(x)) ?? string.Empty);
        }
    }
}
