using PromptEnhancer.Models.Configurations;

namespace PromptEnhancer.Prompt
{
    public static class PromptUtility
    {
        public static string BuildPrompt(PromptConfiguration promptConf, string query, string searchResult)
        {
            //TODO build prompt better, this is temporary
            return @$"""
                    System: {promptConf.SystemInstructions}.
                    Generated output should concise of about {promptConf.TargetOutputLength} words.
                    {(!string.IsNullOrEmpty(promptConf.MacroDefinition) ? $" Any phrase wrapped in {promptConf.MacroDefinition} is a macro and must be kept exactly as-is." : string.Empty)}
                    Used Search Query: {query}
                    Augmented Data: {searchResult}
                    {(!string.IsNullOrEmpty(promptConf.AdditionalInstructions) ? promptConf.AdditionalInstructions : string.Empty)}
                    """;
        }
    }
}
