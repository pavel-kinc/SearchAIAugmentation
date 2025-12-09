namespace PromptEnhancer.ChunkService
{
    /// <summary>
    /// Provides functionality to generate chunks of text from a given raw text input.
    /// </summary>
    public interface IChunkGeneratorService
    {
        /// <summary>
        /// Splits the provided text into chunks of a specified size.
        /// </summary>
        /// <param name="rawText">The text to be divided into chunks. Cannot be null or empty.</param>
        /// <param name="chunkTokenSize">The size of each chunk. If null, a default size is used. Must be a positive integer if specified.</param>
        /// <returns>A list of strings, each representing a chunk of the original text. The last chunk may be smaller if the text
        /// length is not a multiple of the chunk size.</returns>
        public IList<string> GenerateChunksFromData(string rawText, int? chunkTokenSize);
    }
}
