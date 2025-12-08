using PromptEnhancer.KnowledgeBaseCore.Interfaces;
using PromptEnhancer.KnowledgeSearchRequest.Interfaces;

namespace PromptEnhancer.KnowledgeSearchRequest
{
    /// <summary>
    /// Represents a request for performing a knowledge base search with customizable filters and settings.
    /// </summary>
    /// <typeparam name="TFilter">The type of the filter used to refine the search results. Must implement <see
    /// cref="IKnowledgeBaseSearchFilter"/>.</typeparam>
    /// <typeparam name="TSettings">The type of the settings used to configure the search behavior. Must implement <see
    /// cref="IKnowledgeBaseSearchSettings"/>.</typeparam>
    public class KnowledgeSearchRequest<TFilter, TSettings> : IKnowledgeSearchRequest<TFilter, TSettings>
        where TFilter : class, IKnowledgeBaseSearchFilter
        where TSettings : class, IKnowledgeBaseSearchSettings
    {
        /// <inheritdoc/>
        public TFilter? Filter { get; set; }
        /// <inheritdoc/>
        public required TSettings Settings { get; set; }
    }
}
