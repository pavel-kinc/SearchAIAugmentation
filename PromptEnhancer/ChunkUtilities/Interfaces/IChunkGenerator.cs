namespace PromptEnhancer.ChunkUtilities.Interfaces
{
    public interface IChunkGenerator
    {
        public IList<string> GenerateChunksFromData(string rawText);
    }
}
