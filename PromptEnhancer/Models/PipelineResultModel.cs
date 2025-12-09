using ErrorOr;
using PromptEnhancer.Models.Pipeline;

namespace PromptEnhancer.Models
{
    /// <summary>
    /// Represents the result of a pipeline execution, including the outcome and any associated errors.
    /// </summary>
    /// <remarks>This model provides information about the success or failure of a pipeline run.  The <see
    /// cref="Result"/> property contains the details of the pipeline execution,  while the <see cref="Errors"/>
    /// property lists any errors encountered during the run. The <see cref="PipelineSuccess"/> property indicates
    /// whether the pipeline completed successfully.</remarks>
    public class PipelineResultModel
    {
        /// <summary>
        /// Gets the result of the pipeline run. If the pipeline failed, this property may be null.
        /// May be reused in another pipeline executions (Disclaimer: you need to control the steps and surrounding context accordingly).
        /// </summary>
        public PipelineRun? Result { get; init; }

        /// <summary>
        /// Gets the collection of errors associated with the current operation or state.
        /// </summary>
        public IEnumerable<Error> Errors { get; init; } = [];

        /// <summary>
        /// Gets a value indicating whether the pipeline completed successfully.
        /// </summary>
        public bool PipelineSuccess => !Errors.Any();
    }
}
