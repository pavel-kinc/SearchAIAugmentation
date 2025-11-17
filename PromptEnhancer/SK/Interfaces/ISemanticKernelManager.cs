using ErrorOr;
using Microsoft.SemanticKernel;
using PromptEnhancer.Models.Configurations;

namespace PromptEnhancer.SK.Interfaces
{
    public interface ISemanticKernelManager
    {
        public void AddPluginToSemanticKernel<Plugin>(Kernel kernel) where Plugin : class;
        public ErrorOr<Kernel> CreateKernel(IEnumerable<KernelServiceBaseConfig> kernelServiceConfigs, bool addInternalServices = false, bool addContextPlugins = false);
        public ErrorOr<IEnumerable<KernelServiceBaseConfig>> ConvertConfig(KernelConfiguration kernelData);
        //public Task<ChatCompletionResult> GetAICompletionResult(Kernel kernel, string prompt, int? maxPromptLength = null);
    }
}
