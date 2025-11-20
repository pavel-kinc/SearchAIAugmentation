namespace PromptEnhancer.ChunkService
{
    public interface IChunkGeneratorService
    {
        public IList<string> GenerateChunksFromData(string rawText, int? chunkSize);
    }
}
