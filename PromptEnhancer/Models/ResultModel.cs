namespace PromptEnhancer.Models
{
    public class ResultModel
    {
        public string? Query { get; set; }

        public string Prompt { get; set; } = string.Empty;

        public string SearchResult { get; set; } = string.Empty;

        public ChatCompletionResult? AIResult { get; set; }
    }
}
