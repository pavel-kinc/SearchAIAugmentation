namespace PromptEnhancer.Services.RankerService
{
    //TODO dont forget IChunkRankerService, delete or move to some demo
    public interface IRankerService
    {
        //TODO this expects same length vectors!
        float? GetSimilarityScore(ReadOnlyMemory<float> queryEmbed, ReadOnlyMemory<float> recordEmbed);
    }
}
