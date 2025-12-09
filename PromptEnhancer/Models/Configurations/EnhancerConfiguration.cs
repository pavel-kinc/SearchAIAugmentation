using PromptEnhancer.Models.Pipeline;
using PromptEnhancer.Pipeline.Interfaces;

namespace PromptEnhancer.Models.Configurations
{
    /// <summary>
    /// Represents the configuration settings for an enhancer pipeline, including kernel, prompt, and pipeline-specific
    /// configurations. It serves as main configuration class for setting up and using the enhancer pipeline.
    /// </summary>
    /// <remarks>This class provides a centralized configuration for setting up and customizing the behavior
    /// of an enhancer pipeline.  It includes settings for the kernel, prompt, additional pipeline options, and the
    /// sequence of pipeline steps.</remarks>
    public class EnhancerConfiguration
    {
        /// <summary>
        /// Gets or sets the kernel configuration for the system.
        /// </summary>
        public KernelConfiguration? KernelConfiguration { get; set; }
        /// <summary>
        /// Gets or sets the configuration settings for prompts.
        /// </summary>
        public PromptConfiguration PromptConfiguration { get; set; } = new();
        /// <summary>
        /// Gets or sets the additional settings for configuring the pipeline.
        /// </summary>
        public PipelineAdditionalSettings PipelineAdditionalSettings { get; set; } = new();
        /// <summary>
        /// Gets or sets the collection of pipeline steps to be executed.
        /// </summary>
        public IEnumerable<IPipelineStep> Steps { get; set; } = [];
    }
}
