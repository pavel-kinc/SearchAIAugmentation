using PromptEnhancer.KnowledgeRecord.Interfaces;
using PromptEnhancer.Models.Configurations;
using System.Text.RegularExpressions;

namespace PromptEnhancer.Prompt
{
    public static partial class PromptUtility
    {
        [GeneratedRegex(@"(\r?\n){2,}")]
        private static partial Regex CollapseNewLines();
        public const char RecordSeparator = '\n';
        // TODO FINISH ALL FROM PROMPTCONFIG
        // split to User part and System part
        public static string BuildPrompt(PromptConfiguration? promptConf, string? query, IEnumerable<IKnowledgeRecord>? searchResult)
        {
            promptConf ??= new PromptConfiguration();
            //TODO build prompt better, this is temporary
            var res = @$"""
                    System: {promptConf.SystemInstructions}.
                    Generated output should concise of about {promptConf.TargetOutputLength} words.
                    {(!string.IsNullOrEmpty(promptConf.MacroDefinition) ? $" Any phrase wrapped in {promptConf.MacroDefinition} is a macro and must be kept exactly as-is." : string.Empty)}
                    {(!string.IsNullOrEmpty(query) ? $"Used Search Query: {query}" : string.Empty)}
                    {(searchResult is not null && searchResult.Any() ? $"Augmented Data: {string.Join(RecordSeparator, searchResult.Select(x => x.LLMRepresentationString))}" : string.Empty)}
                    {(!string.IsNullOrEmpty(promptConf.AdditionalInstructions) ? promptConf.AdditionalInstructions : string.Empty)}
                    """;
            return CollapseNewLines().Replace(res, "\n");
        }


    }
}
