using PromptEnhancer.KnowledgeBaseCore.Interfaces;

namespace PromptEnhancer.KnowledgeSearchRequest.Interfaces
{
    /// <summary>
    /// Represents a request for performing a knowledge base search with customizable filters and settings.
    /// </summary>
    /// <typeparam name="TFilter">The type of the filter used to refine the search results. Must implement <see
    /// cref="IKnowledgeBaseSearchFilter"/>.</typeparam>
    /// <typeparam name="TSettings">The type of the settings used to configure the search behavior. Must implement <see
    /// cref="IKnowledgeBaseSearchSettings"/>.</typeparam>
    public interface IKnowledgeSearchRequest<TFilter, TSettings> : IKnowledgeSearchRequest
        where TFilter : IKnowledgeBaseSearchFilter
        where TSettings : IKnowledgeBaseSearchSettings
    {
        /// <summary>
        /// Gets or sets the filter criteria used to refine the results (in search). Can be parameters to send with search query.
        /// </summary>
        public TFilter? Filter { get; set; }

        /// <summary>
        /// Gets or sets the settings used to configure the behavior of the component. Like api key, engine, etc.
        /// </summary>
        public TSettings Settings { get; set; }
    }

    /// <summary>
    /// Represents a request to perform a knowledge search operation.
    /// </summary>
    public interface IKnowledgeSearchRequest
    {

    }
}
