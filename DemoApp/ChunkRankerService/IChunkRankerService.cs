namespace ConfigurableGoogleSearchDemo.ChunkRankerService
{
    /// <summary>
    /// Provides functionality to rank and extract the most relevant data from a collection of text chunks based on a
    /// target string.
    /// </summary>
    /// <remarks>This service is designed to analyze a list of text chunks and determine their relevance to a
    /// specified target string. The method allows for limiting the number of top-ranked chunks to extract.</remarks>
    public interface IChunkRankerService
    {
        /// <summary>
        /// Extracts the most relevant data from a collection of text chunks based on their similarity to a target
        /// string.
        /// </summary>
        /// <param name="chunks">A collection of text chunks to analyze. Cannot be null or empty.</param>
        /// <param name="targetString">The target string used to determine relevance. Cannot be null or empty.</param>
        /// <param name="top">The maximum number of top relevant chunks to include in the result. Must be greater than 0. Defaults to 4.</param>
        /// <returns>A single string containing the most relevant chunks concatenated together, or an empty string if no relevant
        /// chunks are found.</returns>
        public string ExtractRelevantDataFromChunks(IList<string> chunks, string targetString, int top = 4);
    }
}
