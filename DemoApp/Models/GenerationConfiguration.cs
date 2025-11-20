namespace DemoApp.Models
{
    public class GenerationConfiguration
    {
        public float? Temperature { get; set; }
        public int MaxTokens { get; set; } = 1000;
        public float? TopP { get; set; }
        public float? FrequencyPenalty { get; set; }
        public float? PresencePenalty { get; set; }
        public int PromptSizeLimit { get; set; } = 10000;
        public bool AllowFunctionCalling { get; set; } = true;

    }
}
