using Microsoft.SemanticKernel;

namespace PromptEnhancer.Plugins.Interfaces
{
    public interface ISemanticKernelPlugin
    {
        void RegisterWithKernel(Kernel kernel);
    }
}
