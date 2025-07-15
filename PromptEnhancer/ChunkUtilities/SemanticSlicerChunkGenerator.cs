using PromptEnhancer.ChunkUtilities.Interfaces;
using SemanticSlicer;

namespace PromptEnhancer.ChunkUtilities
{
    internal class SemanticSlicerChunkGenerator : IChunkGenerator
    {
        private readonly Slicer _slicerInstance;

        public SemanticSlicerChunkGenerator()
        {
            _slicerInstance = new(new SlicerOptions
            {
                MaxChunkTokenCount = 200,
                Separators = Separators.Text,
                StripHtml = false
            });
        }

        public IList<string> GenerateChunksFromData(string rawText)
        {
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
