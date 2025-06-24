using PromptEnhancer.CustomAttributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PromptEnhancer.Models
{
    public class SearchProviderData
    {
        [Display(Name = "Search Api Key:")]
        [Sensitive]
        public string? SearchApiKey { get; set; }
        [Display(Name = "Search Engine:")]
        [Sensitive]
        public string? Engine { get; set; }
        [Display(Name = "Search Provider:")]
        public SearchProviderEnum Provider { get; set; }
    }
}
