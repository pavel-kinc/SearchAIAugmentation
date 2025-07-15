using AllMiniLmL6V2Sharp;
using AllMiniLmL6V2Sharp.Tokenizer;
using PromptEnhancer.ChunkUtilities.Interfaces;
using System.Reflection;

namespace PromptEnhancer.ChunkUtilities
{
    internal class MiniLmL6V2ChunkRanker : IChunkRanker
    {
        private readonly AllMiniLmL6V2Embedder _embedder;

        public MiniLmL6V2ChunkRanker()
        {
            _embedder = new(
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/model/model.onnx",
            new BertTokenizer(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/model/vocab.txt"));
        }

        public string ExtractRelevantDataFromChunks(
        IList<string> chunks,
        string query,
        int top = 4)
        {
            if (chunks == null || chunks.Count == 0 || string.IsNullOrEmpty(query))
            {
                return string.Empty;
            }

            var queryEmbedding = _embedder.GenerateEmbedding(query + " product description").ToArray();

            var chunkEmbeddings = _embedder.GenerateEmbeddings(chunks).ToList();

            var step = chunks
                .Select((chunk, i) => new
                {
                    Text = chunk,
                    Score = CosineSimilarity(chunkEmbeddings[i]!.ToArray(), queryEmbedding)
                })
                .OrderByDescending(x => x.Score);


            var bestChunks = step
                .Take(top)
                .Select(x => x.Text)
                .ToList();

            var res = string.Join(Environment.NewLine, bestChunks);
            return res;
        }

        /// <summary>
        /// Simple cosine similarity calculation between two float vectors.
        /// </summary>
        private static double CosineSimilarity(float[] a, float[] b)
        {
            double dot = 0, magA = 0, magB = 0;
            int length = a.Length;
            for (int i = 0; i < length; i++)
            {
                dot += a[i] * b[i];
                magA += a[i] * a[i];
                magB += b[i] * b[i];
            }

            return dot / (Math.Sqrt(magA) * Math.Sqrt(magB));
        }
    }
}
