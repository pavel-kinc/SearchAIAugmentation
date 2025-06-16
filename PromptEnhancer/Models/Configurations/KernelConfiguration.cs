using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PromptEnhancer.Models.Configurations
{
    public class KernelConfiguration
    {
        public string? Model { get; set; }
        public string? AIApiKey { get; set; }
        public AIProviderEnum Provider { get; set; }
    }
}
