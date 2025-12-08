using ErrorOr;
using PromptEnhancer.Models.Pipeline;

namespace PromptEnhancer.Pipeline.Interfaces
{
    /// <summary>
    /// Represents a step in a processing pipeline that performs an operation asynchronously.
    /// </summary>
    /// <remarks>Each implementation of this interface defines a specific operation to be executed as part of
    /// a pipeline. The step receives pipeline settings and context information to guide its execution.</remarks>
    public interface IPipelineStep
    {
        /// <summary>
        /// Executes the pipeline asynchronously using the specified settings and context.
        /// </summary>
        /// <remarks>This method processes the pipeline based on the provided settings and context. The
        /// operation can be canceled by passing a cancellation token. Ensure that the settings and context are properly
        /// initialized before invoking this method.</remarks>
        /// <param name="settings">The settings that configure the pipeline execution.</param>
        /// <param name="context">The context for the pipeline run, containing relevant data and state.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The result contains a value of <see langword="true"/> if
        /// the pipeline executed successfully; otherwise, an error describing the failure.</returns>
        public Task<ErrorOr<bool>> ExecuteAsync(PipelineSettings settings, PipelineRun context, CancellationToken cancellationToken = default);
    }
}
