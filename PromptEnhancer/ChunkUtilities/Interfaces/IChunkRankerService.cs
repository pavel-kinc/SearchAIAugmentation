namespace PromptEnhancer.ChunkUtilities.Interfaces
{
    public interface IChunkRankerService
    {
        public string ExtractRelevantDataFromChunks(IList<string> chunks, string targetString, int top = 4);
    }
}
