using Microsoft.Extensions.AI;
using System.Numerics.Tensors;

namespace PromptEnhancer.Services.RankerService
{
    public class CosineSimilarityRankerService : IRankerService
    {
        //TODO maybe add some source/name that it is cosine similarity?
        public float? GetSimilarityScore(Embedding<float> queryEmbed, Embedding<float> recordEmbed)
        {
            if (queryEmbed.Dimensions != recordEmbed.Dimensions || queryEmbed.ModelId != recordEmbed.ModelId)
            {
                return null;
            }
            return TensorPrimitives.CosineSimilarity(queryEmbed.Vector.ToArray(), recordEmbed.Vector.ToArray());
        }
    }
}
