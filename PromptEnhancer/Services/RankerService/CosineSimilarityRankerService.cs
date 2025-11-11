using System.Numerics.Tensors;

namespace PromptEnhancer.Services.RankerService
{
    public class CosineSimilarityRankerService : IRankerService
    {
        //TODO maybe add some source/name that it is cosine similarity?
        public float GetSimilarityScore(ReadOnlyMemory<float> queryEmbed, ReadOnlyMemory<float> recordEmbed)
        {
            return TensorPrimitives.CosineSimilarity(queryEmbed.ToArray(), recordEmbed.ToArray());
        }
    }
}
