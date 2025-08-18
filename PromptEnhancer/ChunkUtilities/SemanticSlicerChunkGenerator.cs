using Microsoft.SemanticKernel;
using PromptEnhancer.ChunkUtilities.Interfaces;
using SemanticSlicer;
using System.ComponentModel;

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

        [KernelFunction("generate_chunks_from_string")]
        [Description("Generates chunks from string and returns List of strings")]
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
