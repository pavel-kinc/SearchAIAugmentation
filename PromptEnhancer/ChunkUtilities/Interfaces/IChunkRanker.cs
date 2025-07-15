namespace PromptEnhancer.ChunkUtilities.Interfaces
{
    public interface IChunkRanker
    {
        public string ExtractRelevantDataFromChunks(IList<string> chunks, string query, int top = 4);
    }
}
