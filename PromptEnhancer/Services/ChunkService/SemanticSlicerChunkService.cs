using Microsoft.Extensions.Logging;
using SemanticSlicer;

namespace PromptEnhancer.Services.ChunkService
{

    /// <summary>
    /// Provides functionality to generate text chunks from raw input data using semantic slicing options and SemanticSlicer package.
    /// </summary>
    public class SemanticSlicerChunkService : IChunkGeneratorService
    {
        private readonly ILogger<SemanticSlicerChunkService> _logger;
        private readonly SlicerOptions _slicerOptions;

        public SemanticSlicerChunkService(ILogger<SemanticSlicerChunkService> logger, SlicerOptions? options = null)
        {
            _logger = logger;
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
            _logger.LogInformation("Generated {ChunkCount} chunks from input data.", results.Count);

            return results;
        }
    }
}
