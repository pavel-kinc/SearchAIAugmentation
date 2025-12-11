using PromptEnhancer.KnowledgeRecord;

namespace PromptEnhancer.Tests.TestClasses
{
    public class DummyKnowledgeRecord : KnowledgeRecord<string>
    {
        public override string LLMRepresentationString => $"Dummy: {SourceObject}";
    }
}
