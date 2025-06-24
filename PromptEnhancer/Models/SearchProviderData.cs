using PromptEnhancer.CustomAttributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PromptEnhancer.Models
{
    public class SearchProviderData
    {
        [Sensitive]
        public string? SearchApiKey { get; set; }
        [Sensitive]
        public string? Engine { get; set; }
        public SearchProviderEnum Provider { get; set; }
    }
}
