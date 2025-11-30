using PromptEnhancer.ChunkService;
using PromptEnhancer.KnowledgeBaseCore.Interfaces;
using PromptEnhancer.KnowledgeRecord;
using PromptEnhancer.KnowledgeRecord.Interfaces;
using PromptEnhancer.KnowledgeSearchRequest.Interfaces;

namespace PromptEnhancer.KnowledgeBaseCore
{
    public class KnowledgeBaseDefault<TRecord, TModel> : KnowledgeBase<TRecord, EmptySearchFilter, EmptySearchSettings, EmptyModelFilter<TModel>, TModel>
        where TRecord : KnowledgeRecord<TModel>, new()
        where TModel : class
    {

        public KnowledgeBaseDefault(IChunkGeneratorService? chunkGenerator = null) : base(chunkGenerator)
        {
        }

        public async override Task<IEnumerable<TRecord>> SearchAsync(IKnowledgeSearchRequest<EmptySearchFilter, EmptySearchSettings> request, IEnumerable<string> queriesToSearch, EmptyModelFilter<TModel>? filter = null, CancellationToken ct = default)
        {
            return [];
        }

        public IEnumerable<TRecord> GetRecords(IEnumerable<TModel> data, string queryForEmbed, CancellationToken ct = default)
        {
            return GetKnowledgeRecords(data, null, queryForEmbed, true);
        }
    }
}
