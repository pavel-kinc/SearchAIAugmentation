using ErrorOr;
using PromptEnhancer.Models.Pipeline;

namespace PromptEnhancer.Pipeline.PromptEnhancerSteps
{
    /// <summary>
    /// Represents a pipeline step that performs a post-processing check to validate the state of the pipeline context.
    /// </summary>
    /// <remarks>This step checks the state of the <see cref="PipelineRun"/> context to ensure that certain
    /// conditions are met  after processing. Specifically, it verifies whether there are any picked and retrieved
    /// records in the context.</remarks>
    public class PostProcessCheckStep : PipelineStep
    {
        public PostProcessCheckStep(bool isRequired = false) : base(isRequired) { }

        /// <inheritdoc/>
        protected async override Task<ErrorOr<bool>> ExecuteStepAsync(PipelineSettings settings, PipelineRun context, CancellationToken cancellationToken = default)
        {
            return CheckContextState(context);
        }

        /// <summary>
        /// Checks the state of the provided pipeline run context to determine if it meets the required conditions.
        /// </summary>
        /// <remarks>The method evaluates the state of the <paramref name="context"/> based on its picked
        /// and retrieved records. If both collections contain elements, the method returns <see langword="true"/>.
        /// Otherwise, it invokes <c>FailCondition</c> to return an error.</remarks>
        /// <param name="context">The pipeline run context to evaluate. Must not be null.</param>
        /// <returns>An <see cref="ErrorOr{T}"/> containing <see langword="true"/> if the context meets the required conditions;
        /// otherwise, an error indicating the failure condition.</returns>
        protected virtual ErrorOr<bool> CheckContextState(PipelineRun context)
        {
            if (context.PickedRecords.Any() && context.RetrievedRecords.Any())
            {
                return true;
            }

            return FailCondition();
        }
    }
}
