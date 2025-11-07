using ErrorOr;
using PromptEnhancer.Models.Pipeline;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PromptEnhancer.Pipeline.Interfaces
{
    public interface IPipelineOrchestrator
    {
        public Task<ErrorOr<bool>> RunPipelineAsync(Models.Pipeline.Pipeline pipeline, PipelineContext context);
    }
}
