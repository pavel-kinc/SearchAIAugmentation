using PromptEnhancer.Models.Configurations;
using PromptEnhancer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PromptEnhancer.Prompt
{
    public static class PromptUtility
    {
        public static string BuildPrompt(ResultModel resultView, PromptConfiguration promptConf)
        {
            //TODO build prompt better, this is temporary
            return @$"""
                        System: {promptConf.SystemInstructions}.
                        Generated output should concise of about {promptConf.TargetOutputLength} words.
                        {(!string.IsNullOrEmpty(promptConf.MacroDefinition) ? $" Any phrase wrapped in {promptConf.MacroDefinition} is a macro and must be kept exactly as-is." : string.Empty)}
                        Used Search Query: {resultView.Query}
                        Augmented Data: {resultView.SearchResult}
                        {(!string.IsNullOrEmpty(promptConf.AdditionalInstructions) ? promptConf.AdditionalInstructions : string.Empty)}
                    """;
        }
    }
}
