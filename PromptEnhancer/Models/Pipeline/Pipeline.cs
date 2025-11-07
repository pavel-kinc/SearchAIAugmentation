using Microsoft.SemanticKernel;
using PromptEnhancer.Pipeline.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PromptEnhancer.Models.Pipeline
{
    public class Pipeline(
        PipelineSettings settings,
        IEnumerable<IPipelineStep> steps)
    {
        public PipelineSettings Settings { get; } = settings;

        public IReadOnlyList<IPipelineStep> Steps { get; } = [.. steps];
    }
}
