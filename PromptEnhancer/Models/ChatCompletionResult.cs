namespace PromptEnhancer.Models
{
    public class ChatCompletionResult
    {
        public string? AIOutput { get; set; }
        public IEnumerable<string>? UsedURLs { get; set; }
        public int TokensUsed { get; set; }
    }
}