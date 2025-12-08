using Microsoft.Extensions.AI;

namespace PromptEnhancer.Services.RankerService
{

    /// <summary>
    /// Defines a service for calculating the similarity score between two embeddings.
    /// </summary>
    public interface IRankerService
    {
        /// <inheritdoc/>
        float? GetSimilarityScore(Embedding<float> queryEmbed, Embedding<float> recordEmbed);
    }
}
