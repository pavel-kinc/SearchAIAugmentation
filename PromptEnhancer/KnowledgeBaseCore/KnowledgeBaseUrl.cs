using PromptEnhancer.ChunkService;
using PromptEnhancer.KnowledgeBaseCore.Interfaces;
using PromptEnhancer.KnowledgeRecord;
using PromptEnhancer.KnowledgeRecord.Interfaces;
using PromptEnhancer.Search.Interfaces;

namespace PromptEnhancer.KnowledgeBaseCore
{
    public abstract class KnowledgeBaseUrl<T, TSearchFilter, TSearchSettings, TFilter, TModel> : KnowledgeBase<T, TSearchFilter, TSearchSettings, TFilter, TModel>
        where T : KnowledgeRecord<TModel>, new()
        where TSearchFilter : class, IKnowledgeBaseSearchFilter
        where TSearchSettings : class, IKnowledgeBaseSearchSettings
        where TFilter : class, IModelFilter<TModel>
        where TModel : class
    {
        protected readonly ISearchWebScraper _searchWebScraper;

        protected KnowledgeBaseUrl(ISearchWebScraper searchWebScraper, IChunkGeneratorService chunkGenerator) : base(chunkGenerator)
        {
            _searchWebScraper = searchWebScraper;
        }
    }
}
