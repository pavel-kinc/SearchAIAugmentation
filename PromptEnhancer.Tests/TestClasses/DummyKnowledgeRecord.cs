using PromptEnhancer.KnowledgeRecord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PromptEnhancer.Tests.TestClasses
{
    public class DummyKnowledgeRecord : KnowledgeRecord<string>
    {
        public override string LLMRepresentationString => $"Dummy: {SourceObject}";
    }
}
