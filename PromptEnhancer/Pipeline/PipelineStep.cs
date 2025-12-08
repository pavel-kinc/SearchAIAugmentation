using ErrorOr;
using PromptEnhancer.Models.Pipeline;
using PromptEnhancer.Pipeline.Interfaces;

namespace PromptEnhancer.Pipeline
{
    /// <summary>
    /// Represents an abstract base class for a step in a pipeline, defining the structure and behavior for executing a
    /// pipeline step asynchronously.
    /// </summary>
    public abstract class PipelineStep : IPipelineStep
    {
        protected bool _isRequired = false;
        protected PipelineStep(bool isRequired = false)
        {
            _isRequired = isRequired;
        }

        /// <inheritdoc/>
        /// <remarks>The method first evaluates whether the step's execution conditions are met. If the
        /// conditions are not met  and the step is required, the execution fails. If the conditions are met, the step
        /// is executed asynchronously.  The result depends on the success of the execution and whether the step is
        /// marked as required.</remarks>
        public async Task<ErrorOr<bool>> ExecuteAsync(PipelineSettings settings, PipelineRun context, CancellationToken cancellationToken = default)
        {
            var check = CheckExecutionConditions(context);
            if (check.IsError)
            {
                return check;
            }

            if (check.Value)
            {
                var res = await ExecuteStepAsync(settings, context, cancellationToken);
                return res.IsError ? res : (res.Value ? true : (_isRequired ? FailExecution() : false));
            }
            return _isRequired ? FailExecution() : false;
        }

        /// <summary>
        /// Executes a single step in the pipeline asynchronously.
        /// </summary>
        /// <param name="settings">The settings that configure the pipeline's behavior during execution.</param>
        /// <param name="context">The context for the current pipeline run, containing state and data relevant to the execution.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests, allowing the operation to be canceled.</param>
        /// <returns>A task that represents the asynchronous operation. The result is an <see cref="ErrorOr{T}"/> containing 
        /// <see langword="true"/> if the step was executed successfully, <see langword="false"/> if it was not,  or an
        /// error if the operation failed.</returns>
        protected abstract Task<ErrorOr<bool>> ExecuteStepAsync(PipelineSettings settings, PipelineRun context, CancellationToken cancellationToken = default);

        /// <summary>
        /// Checks whether the execution conditions for the pipeline step execution are met.
        /// </summary>
        /// <remarks>This method is intended to be overridden in derived classes to implement custom
        /// condition checks. By default, it always returns <see langword="true"/>.</remarks>
        /// <param name="context">The context of the current pipeline run, containing relevant execution data.</param>
        /// <returns>An <see cref="ErrorOr{T}"/> containing <see langword="true"/> if the execution conditions are met;
        /// otherwise, an error indicating the reason for failure.</returns>
        protected virtual ErrorOr<bool> CheckExecutionConditions(PipelineRun context)
        {
            return true;
        }

        /// <summary>
        /// Evaluates whether the conditions for this step have failed.
        /// </summary>
        /// <remarks>If the step is marked as required, this method returns a failure result with an error
        /// message.  Otherwise, it returns <see langword="false"/>.</remarks>
        /// <returns>An <see cref="ErrorOr{T}"/> containing <see langword="false"/> if the conditions have not failed,  or an
        /// error result if the step is required and the conditions check fails.</returns>
        protected virtual ErrorOr<bool> FailCondition()
        {
            return _isRequired ? Error.Failure($"{GetType().Name}: Conditions check for this required step failed.") : false;
        }

        /// <summary>
        /// Marks the execution of the current step as failed, optionally providing a reason for the failure.
        /// </summary>
        /// <remarks>This method is intended to be used in scenarios where a step in a process needs to be
        /// explicitly marked as failed.  If the step is required, an error is returned; otherwise, the method returns
        /// <see langword="false"/>.</remarks>
        /// <param name="reason">An optional message describing the reason for the failure. If <see langword="null"/> or empty, a default
        /// failure message is used.</param>
        /// <returns>An <see cref="ErrorOr{T}"/> containing an error if the step is required and fails, or <see
        /// langword="false"/> if the step is not required.</returns>
        protected virtual ErrorOr<bool> FailExecution(string? reason = null)
        {
            return _isRequired ?
                (string.IsNullOrEmpty(reason) ? Error.Failure($"{GetType().Name}: Execution for this required step failed.") : Error.Failure(reason)) :
                false;
        }
    }
}
