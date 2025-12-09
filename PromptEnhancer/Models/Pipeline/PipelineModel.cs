using PromptEnhancer.Pipeline.Interfaces;

namespace PromptEnhancer.Models.Pipeline
{
    /// <summary>
    /// Represents a data processing pipeline composed of configurable settings and a sequence of processing steps.
    /// </summary>
    /// <remarks>This class encapsulates the configuration and structure of a pipeline, including its settings
    /// and the ordered steps that define its behavior. It is designed to be immutable after initialization, ensuring
    /// thread safety and consistency during execution.</remarks>
    /// <param name="settings">The configuration settings for the pipeline. Cannot be <see langword="null"/>.</param>
    /// <param name="steps">The collection of processing steps that define the pipeline's behavior. The steps are executed in the order they
    /// appear in the collection. Cannot be <see langword="null"/> or contain <see langword="null"/> elements.</param>
    public class PipelineModel(
        PipelineSettings settings,
        IEnumerable<IPipelineStep> steps)
    {
        /// <summary>
        /// Gets the settings used to configure the pipeline.
        /// </summary>
        public PipelineSettings Settings { get; } = settings;

        /// <summary>
        /// Gets the collection of pipeline steps that are executed in sequence.
        /// </summary>
        public IReadOnlyList<IPipelineStep> Steps { get; } = [.. steps];
    }
}
