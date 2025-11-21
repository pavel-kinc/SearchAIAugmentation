using PromptEnhancer.ChunkService;
using PromptEnhancer.KnowledgeBaseCore.Interfaces;
using PromptEnhancer.KnowledgeRecord;
using PromptEnhancer.KnowledgeRecord.Interfaces;
using PromptEnhancer.KnowledgeSearchRequest.Interfaces;
using System.Linq.Expressions;
using System.Reflection;

namespace PromptEnhancer.KnowledgeBaseCore
{

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

        public virtual string Description => $"Knowledge base with name {GetType().Name}";

        public abstract Task<IEnumerable<TRecord>> SearchAsync(IKnowledgeSearchRequest<TSearchFilter, TSearchSettings> request, IEnumerable<string> queriesToSearch, TFilter? filter = null, CancellationToken ct = default);

        protected virtual IEnumerable<TRecord> GetKnowledgeRecords(IEnumerable<TModel> data, TFilter? filter, string queryString, bool allowChunking, int chunkTokenSize = 300, int chunkLimit = 10, CancellationToken ct = default)
        {
            if (filter is not null)
            {
                data = filter.FilterRecords(data);
            }
            var dummyRecord = new TRecord();
            //TODO maybe move chunkgenerator check completely elsewhere? now it just skips chunking if not defined, if deleted it throws error in method below
            if (_chunkGenerator is not null && allowChunking && dummyRecord.ChunkSelector is not null)
            {
                return ChunkRecords(data, queryString, dummyRecord.ChunkSelector, chunkTokenSize, chunkLimit);
            }

            return data.Select(x => CreateRecord(x, queryString));
        }

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
                    chunkPropertyInfo!.SetValue(model, chunk);
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

        protected virtual TRecord CreateRecord(TModel o, string queryString)
        {
            //TODO embedding data (Rank, embeddings) here somehow, maybe through concrete knowledgeBase (where there will be property picker)
            // or maybe let it in concrete knowledgeBase to assign the property of concrete KnowledgeRecord onto model's property
            // this needs to be done in override, the embedding and score - mainly embedding can have some user inserted data - like source or model
            return new TRecord
            {
                Id = Guid.NewGuid().ToString(),
                SourceObject = o,
                Source = GetType().Name,
                UsedSearchQuery = queryString,
            };
        }

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
