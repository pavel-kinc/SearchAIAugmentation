using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;
using PromptEnhancer.CustomAttributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PromptEnhancer.Models.Configurations
{
    public class KernelConfiguration
    {
        [Display(Name = "AI Model:")]
        public string? Model { get; set; }
        [Display(Name = "AI Api Key:")]
        [Sensitive]
        public string? AIApiKey { get; set; }
        [Display(Name = "AI Provider:")]
        public AIProviderEnum Provider { get; set; }
    }
}
