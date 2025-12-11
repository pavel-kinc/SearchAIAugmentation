using PromptEnhancer.Models;
using PromptEnhancer.Tests.TestDataFactory;

namespace PromptEnhancer.Services.EmbeddingService.Tests
{
    public class EmbeddingServiceTests
    {
        [Fact]
        public async Task GenerateEmbeddingsForRecordsAsync_ShouldAssignEmbeddings_WhenRecordsHaveNoEmbeddings()
        {
            var records = TestDataFactory.GetRecords().Take(1);
            var embeddingGeneratorMock = EmbeddingMocksUtilities.GetGeneratorMock("");
            var kernelMock = KernelMocks.GetRealKernelWithMocks(embeddingGeneratorMock);

            var service = new EmbeddingService();

            var result = await service.GenerateEmbeddingsForRecordsAsync(
                kernelMock, (IReadOnlyList<KnowledgeRecord.Interfaces.IKnowledgeRecord>)records, null, null, skipGenerationForEmbData: false);

            Assert.True(result);
            Assert.All(records, r => Assert.NotNull(r.Embeddings));
            Assert.All(records, r => Assert.Equal("Mock", r.Embeddings!.EmbeddingModel));
        }

        [Fact]
        public async Task GenerateEmbeddingsForRecordsAsync_ShouldSkip_WhenSkipGenerationForEmbDataIsTrue()
        {
            var record = TestDataFactory.GetRecords().First();
            record.Embeddings = new PipelineEmbeddingsModel
            {
                EmbeddingModel = "SomeOriginalModel",
                EmbeddingVector = new float[] { 0.1f, 0.2f, 0.3f }
            };
            var embeddingGeneratorMock = EmbeddingMocksUtilities.GetGeneratorMock("");
            var kernelMock = KernelMocks.GetRealKernelWithMocks(embeddingGeneratorMock);

            var service = new EmbeddingService();

            var result = await service.GenerateEmbeddingsForRecordsAsync(
                kernelMock, [record], null, null, skipGenerationForEmbData: true);

            Assert.True(result);
            Assert.NotNull(record.Embeddings);
            Assert.Equal("SomeOriginalModel", record.Embeddings.EmbeddingModel);
            Assert.Equal(new float[] { 0.1f, 0.2f, 0.3f }, record.Embeddings.EmbeddingVector);
        }

        [Fact]
        public async Task GenerateEmbeddingsForRecordsAsync_ShouldAsign_WhenSkipGenerationForEmbDataIsTrue()
        {
            var record = TestDataFactory.GetRecords().First();
            var embeddingGeneratorMock = EmbeddingMocksUtilities.GetGeneratorMock("");
            var kernelMock = KernelMocks.GetRealKernelWithMocks(embeddingGeneratorMock);

            var service = new EmbeddingService();

            var result = await service.GenerateEmbeddingsForRecordsAsync(
                kernelMock, [record], null, null, skipGenerationForEmbData: true);

            Assert.True(result);
            Assert.NotNull(record.Embeddings);
        }

        [Fact]
        public async Task GenerateEmbeddingsForRecordsAsync_ShouldSkip_SimilarityScoreAndSkip()
        {
            var record = TestDataFactory.GetRecords().First();
            record.SimilarityScore = 0.5;
            var embeddingGeneratorMock = EmbeddingMocksUtilities.GetGeneratorMock("");
            var kernelMock = KernelMocks.GetRealKernelWithMocks(embeddingGeneratorMock);

            var service = new EmbeddingService();

            var result = await service.GenerateEmbeddingsForRecordsAsync(
                kernelMock, [record], null, null, skipGenerationForEmbData: true);

            Assert.True(result);
            Assert.Null(record.Embeddings);
        }

        [Fact]
        public async Task GenerateEmbeddingsForRecordsAsync_ShouldAssign_SimilarityScoreAndSkipFalse()
        {
            var record = TestDataFactory.GetRecords().First();
            record.SimilarityScore = 0.5;
            var embeddingGeneratorMock = EmbeddingMocksUtilities.GetGeneratorMock("");
            var kernelMock = KernelMocks.GetRealKernelWithMocks(embeddingGeneratorMock);

            var service = new EmbeddingService();

            var result = await service.GenerateEmbeddingsForRecordsAsync(
                kernelMock, [record], null, null, skipGenerationForEmbData: false);

            Assert.True(result);
            Assert.NotNull(record.Embeddings);
        }
    }
}
