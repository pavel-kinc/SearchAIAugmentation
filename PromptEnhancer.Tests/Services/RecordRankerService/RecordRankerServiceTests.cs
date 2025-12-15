using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Moq;
using PromptEnhancer.KnowledgeRecord.Interfaces;
using PromptEnhancer.Services.RankerService;
using PromptEnhancer.Tests.TestClasses;
using PromptEnhancer.Tests.TestDataFactory;

namespace PromptEnhancer.Services.RecordRankerService.Tests
{
    public class RecordRankerServiceTests
    {
        private readonly Mock<IRankerService> _rankerServiceMock;
        private readonly Mock<ILogger<RecordRankerService>> _loggerMock;
        private readonly RecordRankerService _service;

        private const string TestQuery = "Find product by name";
        private const float ExpectedScore = 0.88f;

        public RecordRankerServiceTests()
        {
            _rankerServiceMock = new Mock<IRankerService>();
            // Set the ranker to return a constant score
            _rankerServiceMock.Setup(r => r.GetSimilarityScore(
                It.IsAny<Embedding<float>>(),
                It.IsAny<Embedding<float>>())).Returns(ExpectedScore);

            _loggerMock = new Mock<ILogger<RecordRankerService>>();
            _service = new RecordRankerService(_rankerServiceMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task Ranker_1_RecordWithNullScore_IsScoredSuccessfully()
        {

            var generatorMock = EmbeddingMocksUtilities.GetGeneratorMock(TestQuery);
            var kernel = KernelMocks.GetRealKernelWithMocks(generatorMock);
            var record = new DummyKnowledgeRecord { UsedSearchQuery = "RecQueryA", SimilarityScore = null, Embeddings = EmbeddingMocksUtilities.GetPipelineEmbeddingsModel() };
            var records = new List<IKnowledgeRecord> { record };


            await _service.AssignSimilarityScoreToRecordsAsync(kernel, records, TestQuery);


            Assert.Equal(ExpectedScore, record.SimilarityScore);

            generatorMock.Verify(g => g.GenerateAsync(
                It.Is<IEnumerable<string>>(v => v.Contains(TestQuery)),
                It.IsAny<EmbeddingGenerationOptions?>(),
                It.IsAny<CancellationToken>()),
                Times.Once);

            // Verify the call for the Record's Search Query
            generatorMock.Verify(g => g.GenerateAsync(
                It.Is<IEnumerable<string>>(v => v.Contains("RecQueryA")),
                It.IsAny<EmbeddingGenerationOptions?>(),
                It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Ranker_2_RecordWithExistingScoreAndEmbeddings_IsRecalculated()
        {

            var generatorMock = EmbeddingMocksUtilities.GetGeneratorMock(TestQuery);
            var kernel = KernelMocks.GetRealKernelWithMocks(generatorMock);
            var record = new DummyKnowledgeRecord { UsedSearchQuery = "RecQueryB", SimilarityScore = 0.15, Embeddings = EmbeddingMocksUtilities.GetPipelineEmbeddingsModel() };
            var records = new List<IKnowledgeRecord> { record };


            await _service.AssignSimilarityScoreToRecordsAsync(kernel, records, TestQuery);


            Assert.Equal(ExpectedScore, record.SimilarityScore);
            _rankerServiceMock.Verify(r => r.GetSimilarityScore(It.IsAny<Embedding<float>>(), It.IsAny<Embedding<float>>()), Times.Once);
        }


        [Fact]
        public async Task Ranker_3_RecordWithScoreButNullEmbeddings_IsSkippedByWhereClause()
        {

            var generatorMock = EmbeddingMocksUtilities.GetGeneratorMock(TestQuery);
            var kernel = KernelMocks.GetRealKernelWithMocks(generatorMock);
            // This record is filtered out by the WHERE clause in the service: 
            var record = new DummyKnowledgeRecord { UsedSearchQuery = "SkipMe", SimilarityScore = 0.5, Embeddings = null };
            var records = new List<IKnowledgeRecord> { record };


            await _service.AssignSimilarityScoreToRecordsAsync(kernel, records, TestQuery);


            Assert.Equal(0.5, record.SimilarityScore); // Score remains unchanged
            _rankerServiceMock.Verify(r => r.GetSimilarityScore(It.IsAny<Embedding<float>>(), It.IsAny<Embedding<float>>()), Times.Never);
        }

        [Fact]
        public async Task Ranker_4_RecordWithNullSearchQuery_IsSkippedInsideTryAssignScore()
        {

            var generatorMock = EmbeddingMocksUtilities.GetGeneratorMock(TestQuery);
            var kernel = KernelMocks.GetRealKernelWithMocks(generatorMock);
            var record = new DummyKnowledgeRecord { UsedSearchQuery = null, SimilarityScore = null };
            var records = new List<IKnowledgeRecord> { record };


            await _service.AssignSimilarityScoreToRecordsAsync(kernel, records, TestQuery);


            Assert.Null(record.SimilarityScore);
            _rankerServiceMock.Verify(r => r.GetSimilarityScore(It.IsAny<Embedding<float>>(), It.IsAny<Embedding<float>>()), Times.Never);
        }


        [Fact]
        public async Task Ranker_5_NullQueryString_NoQueryEmbeddingGenerated()
        {

            var generatorMock = EmbeddingMocksUtilities.GetGeneratorMock(TestQuery);
            var kernel = KernelMocks.GetRealKernelWithMocks(generatorMock);
            var record = new DummyKnowledgeRecord { UsedSearchQuery = "RecQuery", SimilarityScore = null };
            var records = new List<IKnowledgeRecord> { record };


            await _service.AssignSimilarityScoreToRecordsAsync(kernel, records, null);

            generatorMock.Verify(g => g.GenerateAsync(
                It.Is<IEnumerable<string>>(v => v.Contains(TestQuery)),
                It.IsAny<EmbeddingGenerationOptions?>(),
                It.IsAny<CancellationToken>()),
                Times.Never());

            // Record search query embed ("RecQuery") should have been generated.
            // We verify that a collection containing "RecQuery" was passed to the interface method exactly once.
            generatorMock.Verify(g => g.GenerateAsync(
                It.Is<IEnumerable<string>>(v => v.Contains("RecQuery")),
                It.IsAny<EmbeddingGenerationOptions?>(),
                It.IsAny<CancellationToken>()),
                Times.Once());
        }

        [Fact]
        public async Task Ranker_6_MultipleRecordsWithSameSearchQuery_EmbeddingGeneratedOnceDueToCaching()
        {

            var generatorMock = EmbeddingMocksUtilities.GetGeneratorMock(TestQuery);
            var kernel = KernelMocks.GetRealKernelWithMocks(generatorMock);
            var records = new List<IKnowledgeRecord>
            {
                new DummyKnowledgeRecord { UsedSearchQuery = "Common Query", SimilarityScore = null },
                new DummyKnowledgeRecord { UsedSearchQuery = "Common Query", SimilarityScore = null }
            };

            await _service.AssignSimilarityScoreToRecordsAsync(kernel, records, "Common Query");

            // Record query embedding ("Common Query") is generated once and reused (caching test)
            generatorMock.Verify(g => g.GenerateAsync(
                It.Is<IEnumerable<string>>(v => v.Contains("Common Query")),
                It.IsAny<EmbeddingGenerationOptions?>(),
                It.IsAny<CancellationToken>()),
                Times.Once());
        }


        [Fact]
        public async Task Ranker_7_RecordWithNullEmbeddings_AssignsNoScoreAndLogsWarning()
        {

            var generatorMock = EmbeddingMocksUtilities.GetGeneratorMock(TestQuery);
            var kernel = KernelMocks.GetRealKernelWithMocks(generatorMock);
            var record = new DummyKnowledgeRecord { UsedSearchQuery = "Q7", Embeddings = null, SimilarityScore = null };
            var records = new List<IKnowledgeRecord> { record };


            await _service.AssignSimilarityScoreToRecordsAsync(kernel, records, TestQuery);


            Assert.Null(record.SimilarityScore);
            _rankerServiceMock.Verify(r => r.GetSimilarityScore(It.IsAny<Embedding<float>>(), It.IsAny<Embedding<float>>()), Times.Never);
            // Verify warning is logged due to missing embeddings
            _loggerMock.Verify(
                x => x.Log(LogLevel.Warning, It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("missing embeddings")), null, (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()),
                Times.Once);
        }

        [Fact]
        public async Task Ranker_8_RankerServiceReturnsNullScore_ScoreIsNotAssignedAndLogsWarning()
        {

            var generatorMock = EmbeddingMocksUtilities.GetGeneratorMock(TestQuery);
            var kernel = KernelMocks.GetRealKernelWithMocks(generatorMock);
            var record = new DummyKnowledgeRecord { UsedSearchQuery = "Query8", SimilarityScore = null, Embeddings = EmbeddingMocksUtilities.GetPipelineEmbeddingsModel() };
            var records = new List<IKnowledgeRecord> { record };
            _rankerServiceMock.Setup(r => r.GetSimilarityScore(It.IsAny<Embedding<float>>(), It.IsAny<Embedding<float>>())).Returns((float?)null);


            await _service.AssignSimilarityScoreToRecordsAsync(kernel, records, TestQuery);


            Assert.Null(record.SimilarityScore);
            _rankerServiceMock.Verify(r => r.GetSimilarityScore(It.IsAny<Embedding<float>>(), It.IsAny<Embedding<float>>()), Times.Once);
            // Verify warning is logged due to computation failure
            _loggerMock.Verify(
                x => x.Log(LogLevel.Warning, It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("could not compute similarity score")), null, (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()),
                Times.Once);
        }

        [Fact]
        public async Task Ranker_9_EmptyRecordsList_ReturnsTrueAndSkipsRanking()
        {

            var generatorMock = EmbeddingMocksUtilities.GetGeneratorMock(TestQuery);
            var kernel = KernelMocks.GetRealKernelWithMocks(generatorMock);
            var records = Enumerable.Empty<IKnowledgeRecord>();


            var result = await _service.AssignSimilarityScoreToRecordsAsync(kernel, records, TestQuery);


            Assert.True(result);
            _rankerServiceMock.Verify(r => r.GetSimilarityScore(It.IsAny<Embedding<float>>(), It.IsAny<Embedding<float>>()), Times.Never);
            // The main query embedding generation must still happen before the loop
            generatorMock.Verify(g => g.GenerateAsync(
                It.Is<IEnumerable<string>>(v => v.Contains(TestQuery)),
                It.IsAny<EmbeddingGenerationOptions?>(),
                It.IsAny<CancellationToken>()),
                Times.Once());
        }
    }
}
