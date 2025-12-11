using Microsoft.Extensions.AI;
using Moq;
using PromptEnhancer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PromptEnhancer.Tests.TestDataFactory
{
    public interface ITestEmbeddingGeneratorProxy
    {
        // Matching the single-string call signature:
        Task<Embedding<float>> GenerateAsync(
            string value,
            EmbeddingGenerationOptions? options = null,
            CancellationToken cancellationToken = default);
    }

    public static class EmbeddingMocksUtilities
    {
        // Fixed embeddings
        public static readonly Embedding<float> QueryEmbed = new(new float[] { 0.1f, 0.2f }) { ModelId = "Mock" };
        public static readonly Embedding<float> RecordAEmbed = new(new float[] { 0.5f, 0.5f }) { ModelId = "Mock" };

        /// <summary>
        /// Simplest Generator Mock: Uses Moq.As<T> to set up the single-string method signature directly.
        /// </summary>
        public static Mock<IEmbeddingGenerator<string, Embedding<float>>> GetGeneratorMock(string queryToMatch)
        {
            var mock = new Mock<IEmbeddingGenerator<string, Embedding<float>>>();

            mock.Setup(g => g.GenerateAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<EmbeddingGenerationOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IEnumerable<string> values, EmbeddingGenerationOptions? opt, CancellationToken ct) =>
                {
                    var val = values.First();
                    var resultEmbed = (val == queryToMatch) ? QueryEmbed : RecordAEmbed;

                    return new GeneratedEmbeddings<Embedding<float>>([resultEmbed]);
                });

            return mock;
        }

        public static Embedding<float> GetEmbeddings(string? modelId = null)
        {
            return QueryEmbed;
        }

        public static PipelineEmbeddingsModel CreateEmbeddings(string source) => new()
        {
            EmbeddingSource = source,
            EmbeddingModel = "Test",
            EmbeddingVector = new ReadOnlyMemory<float>([1.0f, 2.0f])
        };

        public static PipelineEmbeddingsModel GetPipelineEmbeddingsModel(string? modelId = null)
        {
            return new PipelineEmbeddingsModel
            {
                EmbeddingModel = modelId ?? QueryEmbed.ModelId,
                EmbeddingVector = QueryEmbed.Vector
            };
        }
    }
}
