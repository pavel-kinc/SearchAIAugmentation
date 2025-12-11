using Microsoft.Extensions.AI;

namespace PromptEnhancer.Services.RankerService.Tests
{
    public class CosineSimilarityRankerServiceTests
    {
        private readonly CosineSimilarityRankerService _service = new();

        // Helper to create an embedding for testing
        private Embedding<float> CreateEmbed(float[] vector, string modelId = "MockModel")
        {
            return new(vector) { ModelId = modelId };
        }

        [Fact]
        public void Ranker_1_PerfectMatch_ReturnsOne()
        {
            float sqrt2 = (float)Math.Sqrt(2);
            float[] vector = [1.0f / sqrt2, 1.0f / sqrt2, 0.0f];

            var queryEmbed = CreateEmbed(vector);
            var recordEmbed = CreateEmbed(vector);

            float? score = _service.GetSimilarityScore(queryEmbed, recordEmbed);

            Assert.NotNull(score);
            // Asserting very close to 1.0 due to float precision
            Assert.True(score.Value > 0.9999f);
        }

        [Fact]
        public void Ranker_2_OrthogonalVectors_ReturnsZero()
        {
            // vectors should yield a similarity of 0.0.
            float[] queryVector = [1.0f, 0.0f, 0.0f];
            float[] recordVector = [0.0f, 1.0f, 0.0f];

            var queryEmbed = CreateEmbed(queryVector);
            var recordEmbed = CreateEmbed(recordVector);

            float? score = _service.GetSimilarityScore(queryEmbed, recordEmbed);

            Assert.NotNull(score);
            Assert.Equal(0.0f, score.Value, 4); // Check equality up to 4 decimal places
        }

        [Fact]
        public void Ranker_3_MismatchedDimensions_ReturnsNull()
        {
            float[] queryVector = [1.0f, 1.0f, 1.0f];
            float[] recordVector = [1.0f, 1.0f];

            var queryEmbed = CreateEmbed(queryVector);
            var recordEmbed = CreateEmbed(recordVector);

            float? score = _service.GetSimilarityScore(queryEmbed, recordEmbed);

            Assert.Null(score);
        }

        [Fact]
        public void Ranker_4_MismatchedModelIds_ReturnsNull()
        {
            float[] vector = [1.0f, 1.0f];

            var queryEmbed = CreateEmbed(vector, modelId: "Model_A");
            var recordEmbed = CreateEmbed(vector, modelId: "Model_B");

            float? score = _service.GetSimilarityScore(queryEmbed, recordEmbed);

            Assert.Null(score);
        }

        [Fact]
        public void Ranker_5_MismatchedModelIds_ReturnsNull()
        {
            float[] vectorA = [1.0f, 0.0f];

            // Vector B: [1.0, 1.0] (45-degree angle from the X-axis)
            float[] vectorB = [1.0f, 1.0f];

            var queryEmbed = CreateEmbed(vectorA);
            var recordEmbed = CreateEmbed(vectorB);

            float? score = _service.GetSimilarityScore(queryEmbed, recordEmbed);

            Assert.NotNull(score);
            Assert.True(0.7 < score && score < 0.71);
        }
    }
}
