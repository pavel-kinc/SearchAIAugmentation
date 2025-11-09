using PromptEnhancer.KnowledgeBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PromptEnhancer.KnowledgeSearchRequest.Examples
{
    public class GoogleSearchRequest : KnowledgeSearchRequest<GoogleSearchFilterModel, GoogleSettings>
    {
        public GoogleSearchFilterModel? FilterModel { get; set; }
        public required GoogleSettings Settings { get; set; }
    }
}
