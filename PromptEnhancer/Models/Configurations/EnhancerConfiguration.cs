using Microsoft.SemanticKernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PromptEnhancer.Models.Configurations
{
    public class EnhancerConfiguration
    {
        public KernelConfiguration? KernelConfiguration { get; set; }

        public SearchConfiguration SearchConfiguration { get; set; } = new();
        public PromptConfiguration PromptConfiguration { get; set; } = new();
    }
}
