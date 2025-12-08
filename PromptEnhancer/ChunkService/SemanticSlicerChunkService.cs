using SemanticSlicer;

namespace PromptEnhancer.ChunkService
{

    /// <summary>
    /// Provides functionality to generate text chunks from raw input data using semantic slicing options and SemanticSlicer package.
    /// </summary>
    public class SemanticSlicerChunkService : IChunkGeneratorService
    {
        private readonly SlicerOptions _slicerOptions;

        public SemanticSlicerChunkService(SlicerOptions? options = null)
        {
            _slicerOptions = options ?? new SlicerOptions
            {
                MaxChunkTokenCount = 300,
                Separators = Separators.Text,
                StripHtml = false
            };
        }

        /// <inheritdoc/>
        public IList<string> GenerateChunksFromData(string rawText, int? chunkTokenSize)
        {
            var _slicerInstance = new Slicer(new SlicerOptions
            {
                MaxChunkTokenCount = chunkTokenSize ?? _slicerOptions.MaxChunkTokenCount,
                Separators = _slicerOptions.Separators,
                StripHtml = _slicerOptions.StripHtml
            });
            var documentChunks = _slicerInstance.GetDocumentChunks(rawText);
            var results = new List<string>(documentChunks.Count);

            foreach (var chunk in documentChunks)
            {
                results.Add(chunk.Content);
            }

            return results;
        }
    }
}
