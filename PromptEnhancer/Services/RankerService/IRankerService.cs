using Microsoft.Extensions.AI;

namespace PromptEnhancer.Services.RankerService
{
    //TODO dont forget IChunkRankerService, delete or move to some demo
    public interface IRankerService
    {
        //TODO this expects same length vectors!
        float? GetSimilarityScore(Embedding<float> queryEmbed, Embedding<float> recordEmbed);
    }
}
