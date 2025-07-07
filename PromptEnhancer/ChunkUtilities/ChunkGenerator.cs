using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SemanticSlicer;

namespace PromptEnhancer.ChunkUtilities
{
    public static class ChunkGenerator
    {
        private static readonly Slicer SlicerInstance = new(new SlicerOptions
        {
            MaxChunkTokenCount = 200,
            Separators = Separators.Text,
            StripHtml = false
        });

        public static IList<string> GenerateChunksFromData(string rawText)
        {
            var documentChunks = SlicerInstance.GetDocumentChunks(rawText);
            var results = new List<string>(documentChunks.Count);

            foreach (var chunk in documentChunks)
            {
                results.Add(chunk.Content);
            }

            return results;
        }
    }
}
