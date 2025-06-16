using Microsoft.AspNetCore.Mvc;
using PromptEnhancer.Models;
using PromptEnhancer.Models.Configurations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoApp.Models
{
    public class EnhancerConfiguratorViewModel
    {
        public ConfigurationSetup ConfigurationSetup { get; set; } = new();
        public ResultModel? ResultModel { get; set; }

    }
}
