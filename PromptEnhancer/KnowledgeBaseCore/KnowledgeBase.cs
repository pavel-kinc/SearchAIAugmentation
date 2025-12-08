using Mapster;
using PromptEnhancer.ChunkService;
using PromptEnhancer.KnowledgeBaseCore.Interfaces;
using PromptEnhancer.KnowledgeRecord;
using PromptEnhancer.KnowledgeRecord.Interfaces;
using PromptEnhancer.KnowledgeSearchRequest.Interfaces;
using System.Linq.Expressions;
using System.Reflection;

namespace PromptEnhancer.KnowledgeBaseCore
{
    /// <summary>
    /// Represents an abstract base class for a knowledge base that manages and searches records of type <typeparamref
    /// name="TRecord"/> based on models of type <typeparamref name="TModel"/>. Provides functionality for filtering,
    /// chunking, and creating knowledge records.
    /// </summary>
    /// <remarks>This class provides a framework for implementing a knowledge base with customizable search,
    /// filtering, and chunking capabilities. Derived classes must implement the <see cref="SearchAsync"/> method to
    /// define the search behavior.</remarks>
    /// <typeparam name="TRecord">The type of the knowledge record, which must inherit from <see cref="KnowledgeRecord{TModel}"/> and have a
    /// parameterless constructor.</typeparam>
    /// <typeparam name="TSearchFilter">The type of the search filter used to refine search queries, implementing <see
    /// cref="IKnowledgeBaseSearchFilter"/>.</typeparam>
    /// <typeparam name="TSearchSettings">The type of the search settings used to configure search behavior, implementing <see
    /// cref="IKnowledgeBaseSearchSettings"/>.</typeparam>
    /// <typeparam name="TFilter">The type of the model filter used to filter data, implementing <see cref="IModelFilter{TModel}"/>.</typeparam>
    /// <typeparam name="TModel">The type of the data model used to generate knowledge records.</typeparam>
    public abstract class KnowledgeBase<TRecord, TSearchFilter, TSearchSettings, TFilter, TModel> : IKnowledgeBase<TRecord, TSearchFilter, TSearchSettings, TFilter, TModel>
        where TRecord : KnowledgeRecord<TModel>, new()
        where TSearchFilter : class, IKnowledgeBaseSearchFilter
        where TSearchSettings : class, IKnowledgeBaseSearchSettings
        where TFilter : class, IModelFilter<TModel>
        where TModel : class
    {
        protected readonly IChunkGeneratorService? _chunkGenerator;

        protected KnowledgeBase(IChunkGeneratorService? chunkGenerator = null)
        {
            _chunkGenerator = chunkGenerator;
        }

        /// <inheritdoc/>
        public virtual string Description => $"Knowledge base with name {GetType().Name} and data {typeof(TModel).Name} and record {typeof(TRecord).Name}";

        /// <inheritdoc/>
        public abstract Task<IEnumerable<TRecord>> SearchAsync(IKnowledgeSearchRequest<TSearchFilter, TSearchSettings> request, IEnumerable<string> queriesToSearch, TFilter? filter = null, CancellationToken ct = default);

        /// <summary>
        /// Retrieves a collection of knowledge records based on the provided data, filter, and query parameters.
        /// </summary>
        /// <remarks>If chunking is enabled (<paramref name="allowChunking"/> is <see langword="true"/>),
        /// the method attempts to divide the data into smaller chunks based on the specified  <paramref
        /// name="chunkTokenSize"/> and <paramref name="chunkLimit"/>. Chunking is only performed if a chunk generator
        /// is defined and the record type supports chunk selection.</remarks>
        /// <param name="data">The source data from which knowledge records are generated.</param>
        /// <param name="filter">An optional filter to apply to the data before generating records. If <see langword="null"/>, no filtering
        /// is applied.</param>
        /// <param name="queryString">A query string used to customize the generation of knowledge records.</param>
        /// <param name="allowChunking">A value indicating whether the data should be chunked into smaller segments for processing.</param>
        /// <param name="chunkTokenSize">The maximum token size for each chunk, used when chunking is enabled. Defaults to 300.</param>
        /// <param name="chunkLimit">The maximum number of chunks to generate, used when chunking is enabled. Defaults to 10.</param>
        /// <param name="ct">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>An enumerable collection of knowledge records. If chunking is enabled and supported, the records are
        /// generated in chunks; otherwise, they are generated directly from the data.</returns>
        protected virtual IEnumerable<TRecord> GetKnowledgeRecords(IEnumerable<TModel> data, TFilter? filter, string queryString, bool allowChunking, int chunkTokenSize = 300, int chunkLimit = 10, CancellationToken ct = default)
        {
            if (filter is not null)
            {
                data = filter.FilterModelData(data);
            }
            // Check if chunking is allowed and if the record type supports chunk selection
            var dummyRecord = new TRecord();
            if (_chunkGenerator is not null && allowChunking && dummyRecord.ChunkSelector is not null)
            {
                return ChunkRecords(data, queryString, dummyRecord.ChunkSelector, chunkTokenSize, chunkLimit);
            }

            return data.Select(x => CreateRecord(x, queryString));
        }

        /// <summary>
        /// Divides the data into smaller chunks based on the specified chunking logic and generates records for each
        /// chunk.
        /// </summary>
        /// <remarks>This method uses a chunk generator to divide the selected property of each data model
        /// into smaller chunks. If the chunk generator is not defined, a <see cref="NullReferenceException"/> is
        /// thrown. Only the first <paramref name="chunkLimit"/> chunks are processed for each data model.</remarks>
        /// <param name="data">The collection of data models to be chunked.</param>
        /// <param name="queryString">A query string used to create the records.</param>
        /// <param name="chunkSelector">An expression that selects the property of the data model to be chunked.</param>
        /// <param name="chunkSize">The maximum size of each chunk.</param>
        /// <param name="chunkLimit">The maximum number of chunks to generate per data model.</param>
        /// <returns>An enumerable collection of records, where each record corresponds to a chunk of the input data.</returns>
        /// <exception cref="NullReferenceException">Thrown if the chunk generator is not defined.</exception>
        protected virtual IEnumerable<TRecord> ChunkRecords(IEnumerable<TModel> data, string queryString, Expression<Func<TModel, string?>> chunkSelector, int chunkSize, int chunkLimit)
        {
            if (_chunkGenerator is null)
            {
                throw new NullReferenceException("ChunkGenerator was not defined in KnowledgeBase and user tried to use it");
            }

            var result = new List<TRecord>();
            foreach (var model in data)
            {
                var chunkPropertyInfo = GetPropertyInfo(chunkSelector);

                string? chunkProperty = (string?)chunkPropertyInfo?.GetValue(model);
                if (chunkProperty is null)
                {
                    continue;
                }
                var chunks = _chunkGenerator.GenerateChunksFromData(chunkProperty, chunkSize);
                var i = 0;
                foreach (var chunk in chunks)
                {
                    var newModel = model.Adapt<TModel>();
                    chunkPropertyInfo!.SetValue(newModel, chunk);
                    result.Add(CreateRecord(newModel, queryString));
                    i++;
                    if (i >= chunkLimit)
                    {
                        break;
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Creates a new record of type <typeparamref name="TRecord"/> based on the specified model and query string.
        /// </summary>
        /// <remarks>This method generates a unique identifier for the record and assigns metadata such as
        /// the source object, the name of the current type, and the search query string. Override this method in a
        /// derived class to customize the record creation process, such as embedding additional data or modifying the
        /// default behavior.</remarks>
        /// <param name="o">The source model object used to populate the record.</param>
        /// <param name="queryString">The search query string associated with the record.</param>
        /// <returns>A new instance of <typeparamref name="TRecord"/> initialized with the provided model and query string.</returns>
        protected virtual TRecord CreateRecord(TModel o, string queryString)
        {
            return new TRecord
            {
                Id = Guid.NewGuid().ToString(),
                SourceObject = o,
                Source = GetType().Name,
                UsedSearchQuery = queryString,
            };
        }

        /// <summary>
        /// Retrieves the <see cref="PropertyInfo"/> for a property accessed in the specified expression.
        /// </summary>
        /// <param name="expression">An expression that specifies a property of the model. The expression must be a member access expression.</param>
        /// <returns>The <see cref="PropertyInfo"/> of the property accessed in the expression, or <see langword="null"/> if the
        /// expression does not represent a property access.</returns>
        private static PropertyInfo? GetPropertyInfo(Expression<Func<TModel, string?>> expression)
        {
            if (expression.Body is MemberExpression member)
            {
                return member.Member as PropertyInfo;
            }

            return null;
        }
    }

}
