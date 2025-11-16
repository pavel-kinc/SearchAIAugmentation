using PromptEnhancer.ChunkUtilities.Interfaces;
using PromptEnhancer.KnowledgeBaseCore.Interfaces;
using PromptEnhancer.KnowledgeRecord;
using PromptEnhancer.KnowledgeRecord.Interfaces;
using PromptEnhancer.KnowledgeSearchRequest.Interfaces;

namespace PromptEnhancer.KnowledgeBaseCore
{

    public abstract class KnowledgeBase<T, TSearchFilter, TSearchSettings, TFilter, TModel> : IKnowledgeBase<T, TSearchFilter, TSearchSettings, TFilter, TModel>
        where T : KnowledgeRecord<TModel>, new()
        where TSearchFilter : class, IKnowledgeBaseSearchFilter
        where TSearchSettings : class, IKnowledgeBaseSearchSettings
        where TFilter : class, IRecordFilter<TModel>
        where TModel : class
    {
        protected readonly IChunkGeneratorService? _chunkGenerator;

        protected KnowledgeBase(IChunkGeneratorService? chunkGenerator = null)
        {
            _chunkGenerator = chunkGenerator;
        }

        public virtual string Description => $"Knowledge base with name {GetType().Name}";

        public abstract Task<IEnumerable<T>> SearchAsync(IKnowledgeSearchRequest<TSearchFilter, TSearchSettings> request, IEnumerable<string> queriesToSearch, TFilter? filter = null, CancellationToken ct = default);

        protected virtual IEnumerable<T> GetKnowledgeRecords(IEnumerable<TModel> data, TFilter? filter, string queryString, bool allowChunking, Func<TModel, string>? chunkSelector = null, Action<TModel, string>? assignChunkToProperty = null, int chunkSize = 300, int chunkLimit = 10, CancellationToken ct = default)
        {
            if (filter is not null)
            {
                data = filter.FilterRecords(data);
            }
            //TODO maybe move chunkgenerator check completely elsewhere? now it just skips chunking if not defined, if deleted it throws error in method below
            if (_chunkGenerator is not null && allowChunking && chunkSelector is not null && assignChunkToProperty is not null)
            {
                return ChunkRecords(data, queryString, chunkSelector, assignChunkToProperty, chunkSize, chunkLimit);
            }

            return data.Select(x => CreateRecord(x, queryString));
        }

        protected virtual IEnumerable<T> ChunkRecords(IEnumerable<TModel> data, string queryString, Func<TModel, string> chunkSelector, Action<TModel, string> assignChunkToProperty, int chunkSize, int chunkLimit)
        {
            if (_chunkGenerator is null)
            {
                throw new NullReferenceException("ChunkGenerator was not defined in KnowledgeBase and user tried to use it");
            }

            var result = new List<T>();
            foreach (var model in data)
            {
                string chunkProperty = chunkSelector(model);
                var chunks = _chunkGenerator.GenerateChunksFromData(chunkProperty, chunkSize);
                var i = 0;
                foreach (var chunk in chunks)
                {
                    assignChunkToProperty(model, chunk);
                    result.Add(CreateRecord(model, queryString));
                    i++;
                    if (i >= chunkLimit)
                    {
                        break;
                    }
                }
            }
            return result;
        }

        protected virtual T CreateRecord(TModel o, string queryString)
        {
            //TODO embedding data (Rank, embeddings) here somehow, maybe through concrete knowledgeBase (where there will be property picker)
            // or maybe let it in concrete knowledgeBase to assign the property of concrete KnowledgeRecord onto model's property
            // this needs to be done in override, the embedding and score - mainly embedding can have some user inserted data - like source or model
            return new T
            {
                Id = Guid.NewGuid().ToString(),
                SourceObject = o,
                Source = GetType().Name,
                UsedSearchQuery = queryString,
            };
        }
    }

}
