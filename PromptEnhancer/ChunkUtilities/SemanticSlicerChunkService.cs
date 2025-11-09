using Microsoft.SemanticKernel;
using PromptEnhancer.ChunkUtilities.Interfaces;
using SemanticSlicer;
using System.ComponentModel;

namespace PromptEnhancer.ChunkUtilities
{
    internal class SemanticSlicerChunkService : IChunkGeneratorService
    {
        private readonly SlicerOptions _slicerOptions;

        public SemanticSlicerChunkService(SlicerOptions? options)
        {
            _slicerOptions = options ?? new SlicerOptions
            {
                MaxChunkTokenCount = 300,
                Separators = Separators.Text,
                StripHtml = false
            };
        }

        //TODO delete kernel functions
        [KernelFunction("generate_chunks_from_string")]
        [Description("Generates chunks from string and returns List of strings")]
        public IList<string> GenerateChunksFromData(string rawText, int? chunkSize)
        {
            var _slicerInstance = new Slicer(new SlicerOptions
            {
                MaxChunkTokenCount = chunkSize ?? _slicerOptions.MaxChunkTokenCount,
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
