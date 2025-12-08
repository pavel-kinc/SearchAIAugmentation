using Microsoft.Extensions.AI;
using System.Numerics.Tensors;

namespace PromptEnhancer.Services.RankerService
{
    /// <summary>
    /// Provides functionality to calculate the cosine similarity score between two embeddings.
    /// </summary>
    /// <remarks>This service is designed to rank embeddings based on their cosine similarity, which is a
    /// measure of the angular distance between two vectors in a high-dimensional space. The embeddings must have the
    /// same dimensionality and originate from the same model to compute a valid similarity score.</remarks>
    public class CosineSimilarityRankerService : IRankerService
    {
        /// <inheritdoc/>
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
