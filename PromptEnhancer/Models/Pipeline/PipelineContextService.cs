using PromptEnhancer.Pipeline.Interfaces;

namespace PromptEnhancer.Models.Pipeline
{
    public class PipelineContextService : IPipelineContextService
    {
        private PipelineContext? _currentContext;
        public PipelineContext GetCurrentContext()
        {
            if (_currentContext == null)
            {
                _currentContext = new PipelineContext();
            }
            return _currentContext;
        }

        public void SetCurrentContext(PipelineContext context)
        {
            _currentContext = context;
        }
    }
}
