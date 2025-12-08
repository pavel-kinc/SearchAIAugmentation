namespace DemoApp.Models
{
    /// <summary>
    /// Represents the configuration settings for text generation, including parameters that control the behavior and
    /// constraints of the generation process.
    /// </summary>
    /// <remarks>This class provides various options to fine-tune the text generation process, such as
    /// controlling randomness, limiting output length, and applying penalties to influence the generated content.  Use
    /// these settings to customize the behavior of a text generation model according to your application's
    /// requirements.</remarks>
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
