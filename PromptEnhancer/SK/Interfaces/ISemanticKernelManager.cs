using Microsoft.SemanticKernel;
using PromptEnhancer.Models;
using PromptEnhancer.Models.Configurations;

namespace PromptEnhancer.SK.Interfaces
{
    public interface ISemanticKernelManager
    {
        public Kernel? CreateKernel(KernelConfiguration kernelData);
        public Task<ChatCompletionResult> GetAICompletionResult(Kernel kernel, string prompt, int? maxPromptLength = null);
    }
}
