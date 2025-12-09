using PromptEnhancer.ChunkService;
using PromptEnhancer.KnowledgeBaseCore.Interfaces;
using PromptEnhancer.KnowledgeRecord;
using PromptEnhancer.KnowledgeRecord.Interfaces;
using PromptEnhancer.KnowledgeSearchRequest.Interfaces;

namespace PromptEnhancer.KnowledgeBaseCore
{
    /// <summary>
    /// Provides a default implementation of a data knowledge base that operates on records of type <typeparamref
    /// name="TRecord"/>  and models of type <typeparamref name="TModel"/>. This implementation uses empty filters and
    /// settings by default. It is experimental.
    /// </summary>
    /// <remarks>This class simplifies the use of the <see cref="KnowledgeBase{TRecord, TSearchFilter,
    /// TSearchSettings, TModelFilter, TModel}"/>  by providing default implementations for filters and settings, making
    /// it suitable for scenarios where no custom filtering or settings are required.</remarks>
    /// <typeparam name="TRecord">The type of the knowledge record, which must inherit from <see cref="KnowledgeRecord{TModel}"/> and have a
    /// parameterless constructor.</typeparam>
    /// <typeparam name="TModel">The type of the model associated with the knowledge record.</typeparam>
    // [Experimental("This class is experimental and may change in future releases.")]
    internal class KnowledgeBaseDataDefault<TRecord, TModel> : KnowledgeBase<TRecord, EmptySearchFilter, EmptySearchSettings, EmptyModelFilter<TModel>, TModel>
        where TRecord : KnowledgeRecord<TModel>, new()
        where TModel : class
    {

        public KnowledgeBaseDataDefault(IChunkGeneratorService? chunkGenerator = null) : base(chunkGenerator)
        {
        }

        /// <summary>
        /// This method should not be used for data based knowledge bases. It always returns an empty list.
        /// Use only GetRecords or define full fledged KnowledgeBase with proper SearchFilter and SearchSettings.
        /// </summary>
        /// <returns>Empty list</returns>
        public async override Task<IEnumerable<TRecord>> SearchAsync(IKnowledgeSearchRequest<EmptySearchFilter, EmptySearchSettings> request, IEnumerable<string> queriesToSearch, EmptyModelFilter<TModel>? filter = null, CancellationToken ct = default)
        {
            return [];
        }

        /// <summary>
        /// Retrieves a collection of records based on the provided data and query.
        /// </summary>
        /// <remarks>The method processes the input data using the specified query and returns the
        /// resulting records.  If the operation is canceled via the <paramref name="ct"/>, the returned enumeration may
        /// be incomplete.</remarks>
        /// <param name="data">The input collection of models to process.</param>
        /// <param name="queryForEmbed">The query string used to filter or process the data.</param>
        /// <param name="ct">An optional <see cref="CancellationToken"/> to observe while waiting for the operation to complete.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> containing the processed records.</returns>
        public virtual IEnumerable<TRecord> GetRecords(IEnumerable<TModel> data, string queryForEmbed, CancellationToken ct = default)
        {
            return GetKnowledgeRecords(data, null, queryForEmbed, true);
        }
    }
}
