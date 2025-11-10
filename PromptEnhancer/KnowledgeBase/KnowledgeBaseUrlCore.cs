using PromptEnhancer.ChunkUtilities.Interfaces;
using PromptEnhancer.KnowledgeBase.Interfaces;
using PromptEnhancer.KnowledgeRecord;
using PromptEnhancer.KnowledgeRecord.Interfaces;
using PromptEnhancer.Search.Interfaces;

namespace PromptEnhancer.KnowledgeBase
{
    public abstract class KnowledgeBaseUrlCore<T, TSearchFilter, TSearchSettings, TFilter, TModel> : KnowledgeBaseCore<T, TSearchFilter, TSearchSettings, TFilter, TModel>
        where T : KnowledgeRecord<TModel>, new()
        where TSearchFilter : class, IKnowledgeBaseSearchFilter
        where TSearchSettings : class, IKnowledgeBaseSearchSettings
        where TFilter : class, IRecordFilter<TModel>
        where TModel : class
    {
        protected readonly ISearchWebScraper _searchWebScraper;

        protected KnowledgeBaseUrlCore(ISearchWebScraper searchWebScraper, IChunkGeneratorService chunkGenerator) : base(chunkGenerator)
        {
            _searchWebScraper = searchWebScraper;
        }
    }
}
