using PromptEnhancer.KnowledgeRecord.Interfaces;
using PromptEnhancer.Models;
using PromptEnhancer.Models.Configurations;
using System.Text;
using System.Text.RegularExpressions;

namespace PromptEnhancer.Services.PromptBuildingService
{
    public partial class PromptBuildingService : IPromptBuildingService
    {
        public const string NewLinesReplacement = "\n";
        public const string WhiteSpacesReplacement = " ";
        [GeneratedRegex(@"(\r?\n){2,}", RegexOptions.Compiled)]
        private static partial Regex CollapseNewLines();

        [GeneratedRegex(@"[ \t]{2,}", RegexOptions.Compiled)]
        private static partial Regex CollapseWhiteSpaces();

        public string BuildSystemPrompt(PromptConfiguration? promptConfiguration)
        {
            promptConfiguration ??= new PromptConfiguration();
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(@$"{promptConfiguration.SystemInstructions}");
            sb.AppendLine($"Aim for maximum of {promptConfiguration.TargetOutputLength} words (±10%). Be concise.");
            if (!string.IsNullOrWhiteSpace(promptConfiguration.MacroDefinition))
            {
                var d = promptConfiguration.MacroDefinition!;
                sb.AppendLine($"Macros: preserve the entire token verbatim (e.g., {d}macro{d}), including both delimiters and content. Do not translate, rewrite, expand, or remove macros.");
            }

            if (!string.IsNullOrWhiteSpace(promptConfiguration.AdditionalInstructions))
            {
                sb.AppendLine("Additional Instructions:");
                sb.AppendLine(promptConfiguration.AdditionalInstructions!.Trim());
            }
            sb.AppendLine(@$"The output culture must be in {promptConfiguration.TargetLanguageCultureCode}.");
            sb.AppendLine("Ensure coherence and factual correctness.");

            return ApplyRegexReplacement(sb);
        }

        public string BuildUserPrompt(string queryString, IEnumerable<IKnowledgeRecord> pickedRecords, IEnumerable<string> additionalContext, Entry? entry)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"User Query: {queryString}");

            if (pickedRecords.Any())
            {
                sb.AppendLine("Augmented data (context):");
                foreach (var record in pickedRecords)
                {
                    sb.AppendLine(record.LLMRepresentationString);
                }
            }

            if (additionalContext.Any())
            {
                sb.AppendLine("Additional context for query:");
                foreach (var context in additionalContext)
                {
                    sb.AppendLine(context);
                }
            }

            if (entry is not null)
            {
                _ = entry.EntryOriginalText is not null ? sb.AppendLine($"Original text (for query): {entry.EntryOriginalText}") : sb;
                _ = entry.EntryAdditionalData is not null ? sb.AppendLine($"Additional data (for query): {entry.EntryAdditionalData}") : sb;
            }

            return ApplyRegexReplacement(sb);
        }

        private static string ApplyRegexReplacement(StringBuilder sb)
        {
            return CollapseWhiteSpaces().Replace(CollapseNewLines().Replace(sb.ToString(), NewLinesReplacement), WhiteSpacesReplacement);
        }
    }
}
