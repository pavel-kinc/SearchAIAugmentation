using Microsoft.Extensions.VectorData;

namespace PromptEnhancer.Services
{
    public class VectorStore
    {
        [VectorStoreKey]
        public string Id { get; set; } = default!;

        [VectorStoreData]
        public string Text { get; set; } = default!;

        [VectorStoreVector(Dimensions: 1536)]
        public ReadOnlyMemory<float> Embedding { get; set; } = default!;
    }
}
