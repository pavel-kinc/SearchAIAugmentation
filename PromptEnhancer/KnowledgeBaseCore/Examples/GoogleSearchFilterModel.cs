using Microsoft.SemanticKernel.Data;
using PromptEnhancer.KnowledgeBaseCore.Interfaces;

namespace PromptEnhancer.KnowledgeBaseCore
{
    /// <summary>
    /// Represents a model for filtering Google Custom Search results.
    /// </summary>
    /// <remarks>for info about properties look at https://developers.google.com/custom-search/v1/reference/rest/v1/cse/list</remarks>
    public class GoogleSearchFilterModel : IKnowledgeBaseSearchFilter
    {
        public string? CountryCode { get; init; }

        public string? DateRestrict { get; init; }

        public string? ExactTerms { get; init; }

        public string? ExcludeTerms { get; init; }

        public string? InterfaceLanguage { get; init; }
        public string? LinkSite { get; init; }

        public string? LanguageRestrict { get; init; }

        public string? Rights { get; init; }

        public string? SiteSearch { get; init; }

        public int Top { get; init; } = 3;

#pragma warning disable SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
        /// <summary>
        /// Builds and returns a <see cref="TextSearchOptions"/> object configured with the current search parameters.
        /// </summary>
        /// <returns>A <see cref="TextSearchOptions"/> object containing the configured search parameters. If no parameters are
        /// set, the <see cref="TextSearchOptions.Filter"/> property will be <c>null</c>.</returns>
        public TextSearchOptions BuildParameters()
        {
            TextSearchFilter? filter = null;

            void Add(string key, object value)
            {
                filter ??= new TextSearchFilter();
                filter.Equality(key, value);
            }

            if (CountryCode != null) Add("cr", CountryCode);
            if (DateRestrict != null) Add("dateRestrict", DateRestrict);
            if (ExactTerms != null) Add("exactTerms", ExactTerms);
            if (ExcludeTerms != null) Add("excludeTerms", ExcludeTerms);
            if (InterfaceLanguage != null) Add("hl", InterfaceLanguage);
            if (LinkSite != null) Add("linkSite", LinkSite);
            if (LanguageRestrict != null) Add("lr", LanguageRestrict);
            if (Rights != null) Add("rights", Rights);
            if (SiteSearch != null) Add("siteSearch", SiteSearch);

            var options = new TextSearchOptions
            {
                Top = Top,
                Filter = filter
            };

            return options;
        }
#pragma warning restore SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
    }
}
