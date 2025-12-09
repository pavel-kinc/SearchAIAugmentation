using PromptEnhancer.ChunkService;
using PromptEnhancer.KnowledgeBaseCore.Interfaces;
using PromptEnhancer.KnowledgeRecord;
using PromptEnhancer.KnowledgeRecord.Interfaces;
using PromptEnhancer.Search.Interfaces;

namespace PromptEnhancer.KnowledgeBaseCore
{
    /// <summary>
    /// Represents an abstract base class for a knowledge base that retrieves and processes data from URLs using a web
    /// scraping mechanism. This class provides a foundation for implementing URL-based knowledge bases with
    /// customizable search filters, search settings, and data models.
    /// </summary>
    /// <remarks>This class is designed to be extended by concrete implementations that define specific
    /// behavior for interacting with web-based knowledge sources. It relies on an <see cref="ISearchWebScraper"/> to
    /// perform web scraping operations and a <see cref="IChunkGeneratorService"/> to process and chunk data.</remarks>
    /// <typeparam name="T">The type of knowledge record used in the knowledge base. Must inherit from <see cref="KnowledgeRecord{TModel}"/>
    /// and have a parameterless constructor.</typeparam>
    /// <typeparam name="TSearchFilter">The type of search filter used to refine search queries. Must implement <see
    /// cref="IKnowledgeBaseSearchFilter"/>.</typeparam>
    /// <typeparam name="TSearchSettings">The type of search settings used to configure search behavior. Must implement <see
    /// cref="IKnowledgeBaseSearchSettings"/>.</typeparam>
    /// <typeparam name="TFilter">The type of model filter used to filter data models. Must implement <see cref="IModelFilter{TModel}"/>.</typeparam>
    /// <typeparam name="TModel">The type of data model used in the knowledge base. Must be a reference type.</typeparam>
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
