using AllMiniLmL6V2Sharp;
using AllMiniLmL6V2Sharp.Tokenizer;
using System.Reflection;

namespace ConfigurableGoogleSearchDemo.ChunkRankerService
{
    /// <summary>
    /// Provides functionality to rank and extract the most relevant text chunks based on their semantic similarity to a
    /// target string. Testing purposes.
    /// </summary>
    /// <remarks>This class uses a pre-trained MiniLM-L6-V2 model to generate embeddings for the target string
    /// and the provided text chunks. It calculates the cosine similarity between the embeddings to determine the
    /// relevance of each chunk.</remarks>
    public class MiniLmL6V2ChunkRanker : IChunkRankerService
    {
        private readonly AllMiniLmL6V2Embedder _embedder;

        // requires model files in /model folder based on the executing assembly location
        public MiniLmL6V2ChunkRanker()
        {
            _embedder = new(
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/model/model.onnx",
            new BertTokenizer(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/model/vocab.txt"));
        }

        /// <inheritdoc/>
        public string ExtractRelevantDataFromChunks(
        IList<string> chunks,
        string targetString,
        int top = 5)
        {
            if (chunks == null || chunks.Count == 0 || string.IsNullOrEmpty(targetString))
            {
                return string.Empty;
            }

            var queryEmbedding = _embedder.GenerateEmbedding(targetString + " product description").ToArray();

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
