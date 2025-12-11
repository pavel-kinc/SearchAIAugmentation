using Microsoft.Extensions.Logging;
using Moq;
using PromptEnhancer.KnowledgeRecord.Interfaces;
using PromptEnhancer.Models;
using PromptEnhancer.Tests.TestDataFactory;

namespace PromptEnhancer.Services.RecordPickerService.Tests
{
    public class RecordPickerServiceTests
    {
        private readonly RecordPickerService _service;
        private readonly Mock<ILogger<RecordPickerService>> _loggerMock = new();

        public RecordPickerServiceTests()
        {
            _service = new RecordPickerService(_loggerMock.Object);
        }

        // Test 1: Baseline Test - No options, should return the collection unchanged.
        [Fact]
        public async Task Picker_1_NoOptions_ReturnsOriginalCollection()
        {
            var records = TestDataFactory.GetTestRecords().ToList();
            var filter = new RecordPickerOptions();

            var result = await _service.GetPickedRecordsBasedOnFilter(filter, records);

            Assert.Equal(records.Count, result.Count());
            Assert.Equal(records, result);
        }

        [Theory]
        [InlineData(0.8, 2)] // Includes 0.8 and 0.9
        [InlineData(0.9, 1)] // Only includes 0.9
        [InlineData(0.5, 4)] // Includes all scored records, excludes the null score
        public async Task Picker_2_Theory_FiltersByMinScoreSimilarity(double minScore, int expectedCount)
        {
            var records = TestDataFactory.GetTestRecords();
            var filter = new RecordPickerOptions { MinScoreSimilarity = minScore };

            var result = await _service.GetPickedRecordsBasedOnFilter(filter, records);

            Assert.Equal(expectedCount, result.Count());
        }

        [Theory]
        [InlineData("AlphaSource", 2)] // Two records match "AlphaSource"
        [InlineData("BetaSource", 1)] // One record matches "BetaSource"
        [InlineData("NonExistent", 0)] // Zero records match
        public async Task Picker_3_Theory_FiltersByEmbeddingSource(string source, int expectedCount)
        {
            var records = TestDataFactory.GetTestRecords();
            var filter = new RecordPickerOptions { EmbeddingSourceEquals = source };

            var result = await _service.GetPickedRecordsBasedOnFilter(filter, records);

            Assert.Equal(expectedCount, result.Count());
            // Ensures null embeddings record is excluded
            Assert.DoesNotContain(result, r => r.Source == "Delta");
        }

        [Fact]
        public async Task Picker_4_OrderByScoreDescending_OrdersCorrectly()
        {
            var records = TestDataFactory.GetTestRecords();
            var filter = new RecordPickerOptions { OrderByScoreDescending = true };

            var result = await _service.GetPickedRecordsBasedOnFilter(filter, records);

            // Expected order of scored items: 0.9, 0.8, 0.7, 0.6
            var scores = result.Select(r => r.SimilarityScore).Where(s => s.HasValue).ToList();
            Assert.Equal(0.9, scores.First());
            Assert.Equal(0.6, scores.Last());
            Assert.True(scores.SequenceEqual(scores.OrderByDescending(s => s)), "Scores should be in strict descending order.");
        }

        [Fact]
        public async Task Picker_5_OrderByClauses_OrdersBySourceAscending()
        {
            var records = TestDataFactory.GetTestRecords();
            var filter = new RecordPickerOptions
            {
                // Order by the Source property alphabetically, ascending.
                OrderByClauses = [((r) => r.Source!, false)]
            };

            var result = await _service.GetPickedRecordsBasedOnFilter(filter, records);

            // Expected order: Alpha, Alpha, Beta, Delta, Gamma
            var sources = result.Select(r => r.Source).ToList();
            Assert.Equal("Alpha", sources[0]);
            Assert.Equal("Gamma", sources.Last());
            Assert.True(sources.SequenceEqual(sources.OrderBy(s => s)), "Sources should be in alphabetical order.");
        }

        [Theory]
        [InlineData(1, 2, 2)] // Skip 1, Take 2
        [InlineData(3, 1, 1)] // Skip 3, Take 1
        [InlineData(4, 5, 1)] // Skip 4, Take 5 (only 1 remains)
        public async Task Picker_6_Theory_AppliesSkipAndTake(int skip, int take, int expectedCount)
        {
            var records = TestDataFactory.GetTestRecords();
            var filter = new RecordPickerOptions { Skip = skip, Take = take };

            var result = await _service.GetPickedRecordsBasedOnFilter(filter, records);

            Assert.Equal(expectedCount, result.Count());
        }

        [Fact]
        public async Task Picker_7_Combination_AppliesAllOptions()
        {
            var records = TestDataFactory.GetTestRecords();
            var filter = new RecordPickerOptions
            {
                MinScoreSimilarity = 0.7,
                OrderByScoreDescending = true,
                Skip = 1,
                Take = 1
            };

            var result = await _service.GetPickedRecordsBasedOnFilter(filter, records);

            Assert.Single(result);
            Assert.Equal(0.8, result.First().SimilarityScore);
        }

        [Fact]
        public async Task Picker_8_CustomPredicate_FiltersBySourceObject()
        {
            // Predicate: Only include records where Source is exactly "Alpha" (2 records)
            var records = TestDataFactory.GetTestRecords();
            var predicate = new Func<IKnowledgeRecord, bool>(r => r.Source == "Alpha");

            var filter = new RecordPickerOptions
            {
                Predicate = [predicate]
            };

            var result = await _service.GetPickedRecordsBasedOnFilter(filter, records);

            Assert.Equal(2, result.Count());
            Assert.All(result, r => Assert.Equal("Alpha", r.Source));
        }
    }
}
