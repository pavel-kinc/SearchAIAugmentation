using PromptEnhancer.KnowledgeBaseCore.Interfaces;
using PromptEnhancer.KnowledgeRecord;
using PromptEnhancer.KnowledgeRecord.Interfaces;

namespace PromptEnhancer.KnowledgeBaseCore
{
    /// <summary>
    /// Represents a container for managing and searching knowledge base data, with support for generic record and model
    /// types. It is experimental container for bases, that just hold data and use default knowledge base implementation to work with it.
    /// </summary>
    /// <remarks>This class provides functionality to store and search knowledge base data using a specified
    /// knowledge base implementation. It relies on a default knowledge base implementation to perform operations on the
    /// provided data.</remarks>
    /// <typeparam name="TRecord">The type of the knowledge record, which must inherit from <see cref="KnowledgeRecord{TModel}"/> and have a
    /// parameterless constructor.</typeparam>
    /// <typeparam name="TModel">The type of the model associated with the knowledge record, which must be a reference type.</typeparam>
    public class KnowledgeBaseDataContainer<TRecord, TModel> : IKnowledgeBaseContainer
        where TRecord : KnowledgeRecord<TModel>, new()
        where TModel : class
    {
        private readonly KnowledgeBaseDataDefault<TRecord, TModel> _knowledgeBase;
        private readonly IEnumerable<TModel> _data;

        public KnowledgeBaseDataContainer(KnowledgeBaseDataDefault<TRecord, TModel> knowledgeBaseDefault, IEnumerable<TModel> data)
        {
            _knowledgeBase = knowledgeBaseDefault;
            _data = data;
        }

        /// <inheritdoc/>
        public virtual string Description => _knowledgeBase.Description;

        /// <inheritdoc/>
        public async virtual Task<IEnumerable<IKnowledgeRecord>> SearchAsync(IEnumerable<string> queriesToSearch, CancellationToken ct = default)
        {
            return _knowledgeBase.GetRecords(_data, queriesToSearch.FirstOrDefault(x => !string.IsNullOrEmpty(x)) ?? string.Empty);
        }
    }
}
