using Microsoft.SemanticKernel.Data;
using PromptEnhancer.KnowledgeBase.Interfaces;
using System.Globalization;

namespace PromptEnhancer.KnowledgeBase
{
    public class GoogleSearchFilterModel : IKnowledgeBaseSearchFilter
    {
        public string? CountryCode { get; set; }

        public string? DateRestrict { get; set; }

        public string? ExactTerms { get; set; }

        public List<string>? ExcludeTerms { get; set; }

        public string? InterfaceLanguage { get; set; }
        public string? LinkSite { get; set; }

        public string? LanguageRestrict { get; set; }

        public string? Rights { get; set; }

        public string? SiteSearch { get; set; }

        public int Top { get; set; } = 3;

#pragma warning disable SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
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
