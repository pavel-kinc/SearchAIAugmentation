using Microsoft.SemanticKernel;
using PromptEnhancer.Plugins.Interfaces;

namespace PromptEnhancer.Plugins
{
    public abstract class SemanticKernelPlugin : ISemanticKernelPlugin
    {
        public virtual void RegisterWithKernel(Kernel kernel)
        {
            kernel.Plugins.AddFromObject(this, GetType().Name);
        }
    }
}
