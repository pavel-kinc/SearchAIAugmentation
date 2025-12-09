using ErrorOr;
using PromptEnhancer.Models.Pipeline;

namespace PromptEnhancer.Pipeline.Interfaces
{
    /// <summary>
    /// Defines the contract for orchestrating the execution of a pipeline.
    /// </summary>
    /// <remarks>Implementations of this interface are responsible for coordinating the execution of a
    /// pipeline based on the provided pipeline model and execution context. The orchestration process may involve
    /// invoking multiple stages or steps within the pipeline.</remarks>
    public interface IPipelineOrchestrator
    {
        /// <summary>
        /// Executes the specified pipeline asynchronously within the given context.
        /// </summary>
        /// <remarks>The method processes the pipeline in the provided context and returns a result
        /// indicating success or failure.  If the operation is canceled via the <paramref name="ct"/> token, the task
        /// will be marked as canceled.</remarks>
        /// <param name="pipeline">The pipeline model that defines the sequence of operations to execute.</param>
        /// <param name="context">The context for the pipeline run, including any runtime parameters or state.</param>
        /// <param name="ct">An optional <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The result is an <see cref="ErrorOr{T}"/> containing 
        /// <see langword="true"/> if the pipeline executed successfully; otherwise, an error describing the failure.</returns>
        public Task<ErrorOr<bool>> RunPipelineAsync(PipelineModel pipeline, PipelineRun context, CancellationToken ct = default);
    }
}
