using PromptEnhancer.ChunkUtilities.Interfaces;
using PromptEnhancer.Search.Interfaces;

namespace PromptEnhancer.KnowledgeBase
{
    public class KnowledgeBaseUrlBase : KnowledgeBaseBase
    {
        protected readonly ISearchWebScraper _searchWebScraper;

        protected KnowledgeBaseUrlBase(ISearchWebScraper searchWebScraper, IChunkGenerator chunkGenerator) : base(chunkGenerator)
        {
            _searchWebScraper = searchWebScraper;
        }
    }
}
