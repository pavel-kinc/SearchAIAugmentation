namespace PromptEnhancer.ChunkUtilities.Interfaces
{
    public interface IChunkGeneratorService
    {
        public IList<string> GenerateChunksFromData(string rawText, int? chunkSize);
    }
}
