using Microsoft.SemanticKernel;
using PromptEnhancer.Models;
using PromptEnhancer.Models.Configurations;

namespace PromptEnhancer.SK.Interfaces
{
    public interface ISemanticKernelManager
    {
        public void AddPluginToSemanticKernel<Plugin>(Kernel kernel) where Plugin : class;
        public Kernel? CreateKernel(KernelConfiguration kernelData);
        public Task<ChatCompletionResult> GetAICompletionResult(Kernel kernel, string prompt, int? maxPromptLength = null);
    }
}
