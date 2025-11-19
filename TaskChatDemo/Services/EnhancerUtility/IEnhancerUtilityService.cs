using ErrorOr;
using Microsoft.SemanticKernel;
using PromptEnhancer.Models;
using PromptEnhancer.Models.Pipeline;

namespace TaskChatDemo.Services.EnhancerUtility
{
    public interface IEnhancerUtilityService
    {
        Task<PipelineContext> GetContextFromPipeline(string q, bool skipPipeline, Entry entry, PipelineSettings settings);
        ErrorOr<PipelineSettings> GetPipelineSettings();
    }
}
