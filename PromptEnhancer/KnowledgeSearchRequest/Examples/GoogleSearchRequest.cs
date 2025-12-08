using PromptEnhancer.KnowledgeBaseCore;

namespace PromptEnhancer.KnowledgeSearchRequest.Examples
{
    /// <summary>
    /// Represents a request to perform a search using Google, with support for filtering and configuration settings.
    /// </summary>
    /// <remarks>This class extends <see cref="KnowledgeSearchRequest{TFilter, TSettings}"/> to provide
    /// functionality specific to Google searches.  Use this class to define the filters and settings required for
    /// executing a search query against Google.</remarks>
    public class GoogleSearchRequest : KnowledgeSearchRequest<GoogleSearchFilterModel, GoogleSettings>
    {
    }
}
