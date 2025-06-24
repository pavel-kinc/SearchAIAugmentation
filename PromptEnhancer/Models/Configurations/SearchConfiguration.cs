using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PromptEnhancer.Models.Configurations
{
    public class SearchConfiguration
    {
        public SearchProviderData SearchProviderData { get; set; } = new();

        [Display(Name = "Query for Search:")]
        public string? QueryString { get; set; }
    }
}
