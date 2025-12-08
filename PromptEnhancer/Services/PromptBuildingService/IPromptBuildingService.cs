using PromptEnhancer.KnowledgeRecord.Interfaces;
using PromptEnhancer.Models;
using PromptEnhancer.Models.Configurations;

namespace PromptEnhancer.Services.PromptBuildingService
{
    /// <summary>
    /// Provides methods for constructing system and user prompts based on specified configurations and context.
    /// </summary>
    public interface IPromptBuildingService
    {
        /// <summary>
        /// Builds a system prompt string based on the specified configuration.
        /// </summary>
        /// <remarks>The returned prompt string is tailored according to the provided configuration,
        /// allowing for  customization of its content and format. If no configuration is supplied, a default prompt is
        /// generated.</remarks>
        /// <param name="promptConfiguration">An optional <see cref="PromptConfiguration"/> object that defines the settings and parameters  for
        /// constructing the system prompt. If null, default settings are used.</param>
        /// <returns>A string representing the constructed system prompt.</returns>
        public string BuildSystemPrompt(PromptConfiguration? promptConfiguration);

        /// <summary>
        /// Constructs a user-facing prompt based on the provided query, selected knowledge records, additional context,
        /// and an optional entry.
        /// </summary>
        /// <param name="queryString">The query string provided by the user, which serves as the basis for the prompt.</param>
        /// <param name="pickedRecords">A collection of knowledge records selected to provide context or information for the prompt.</param>
        /// <param name="additionalContext">A collection of additional context strings to further refine or enhance the prompt.</param>
        /// <param name="entry">An optional entry object that may provide supplementary details or context for the prompt. Can be <see
        /// langword="null"/>.</param>
        /// <returns>A string representing the constructed user prompt, incorporating the query, selected records, additional
        /// context, and optional entry details.</returns>
        public string BuildUserPrompt(string queryString, IEnumerable<IKnowledgeRecord> pickedRecords, IEnumerable<string> additionalContext, Entry? entry);
    }
}
