namespace PromptEnhancer.ChunkUtilities.Interfaces
{
    public interface IChunkRanker
    {
        public string ExtractRelevantDataFromChunks(IList<string> chunks, string targetString, int top = 4);
    }
}
