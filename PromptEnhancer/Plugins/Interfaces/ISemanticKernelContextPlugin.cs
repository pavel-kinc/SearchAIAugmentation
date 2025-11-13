namespace PromptEnhancer.Plugins.Interfaces
{
    public interface ISemanticKernelContextPlugin : ISemanticKernelPlugin
    {
        public string PluginMethodStartsWith => "get_context";
    }
}
