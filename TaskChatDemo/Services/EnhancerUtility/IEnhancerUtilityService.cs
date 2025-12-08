using ErrorOr;
using PromptEnhancer.Models;
using PromptEnhancer.Models.Pipeline;

namespace TaskChatDemo.Services.EnhancerUtility
{
    /// <summary>
    /// Provides utility methods for enhancing and managing pipeline operations.
    /// </summary>
    public interface IEnhancerUtilityService
    {
        /// <summary>
        /// Retrieves the context of a pipeline run based on the specified query and settings.
        /// </summary>
        /// <param name="q">The query string used to search relevant context (user input).</param>
        /// <param name="skipPipeline">A boolean value indicating whether to bypass the pipeline execution. <see langword="true"/> to skip
        /// the pipeline; otherwise, <see langword="false"/>.</param>
        /// <param name="entry">The entry object that provides additional context or data for the pipeline run.</param>
        /// <param name="settings">The settings that configure the behavior and parameters of the pipeline.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the          <see
        /// cref="PipelineRun"/> object representing the context of the pipeline run.</returns>
        Task<PipelineRun> GetContextFromPipeline(string q, bool skipPipeline, Entry entry, PipelineSettings settings);

        /// <summary>
        /// Retrieves the pipeline settings.
        /// </summary>
        /// <returns>An <see cref="ErrorOr{PipelineSettings}"/> object containing the pipeline settings if successful; otherwise,
        /// an error indicating the failure reason.</returns>
        ErrorOr<PipelineSettings> GetPipelineSettings();
    }
}
