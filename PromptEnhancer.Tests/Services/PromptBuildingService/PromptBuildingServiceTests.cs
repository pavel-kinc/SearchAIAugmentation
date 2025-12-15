using Microsoft.Extensions.Logging;
using Moq;
using PromptEnhancer.KnowledgeRecord.Interfaces;
using PromptEnhancer.Tests.TestDataFactory;

namespace PromptEnhancer.Services.PromptBuildingService.Tests
{
    public class PromptBuildingServiceTests
    {
        private readonly Mock<ILogger<PromptBuildingService>> _loggerMock;
        private readonly PromptBuildingService _service;

        public PromptBuildingServiceTests()
        {
            _loggerMock = new Mock<ILogger<PromptBuildingService>>();
            _service = new PromptBuildingService(_loggerMock.Object);
        }

        [Fact]
        public void SystemPrompt_1_FullConfiguration_CorrectWorkflow()
        {
            var config = TestDataFactory.FullPromptConfiguration();
            var result = _service.BuildSystemPrompt(config);

            // Check for presence of all key parts based on factory data
            Assert.Contains(config.SystemInstructions, result);
            Assert.Contains($"Aim for maximum of {config.TargetOutputLength} words (±10%). Be concise.", result);
            Assert.Contains($"Macros: preserve the entire token verbatim (e.g., {config.MacroDefinition}", result);
            Assert.Contains($"Additional Instructions:", result);
            Assert.Contains(config.AdditionalInstructions!, result);
            Assert.Contains($"The output culture must be in {config.TargetLanguageCultureCode}.", result);
        }

        [Fact]
        public void SystemPrompt_2_NullConfiguration_UsesDefaultsAndExcludesSections()
        {
            var result = _service.BuildSystemPrompt(null);

            // Assertions based on default PromptConfiguration() values
            Assert.Contains("Aim for maximum of", result);
            Assert.Contains("The output culture must be in en-US.", result);

            Assert.DoesNotContain("Macros:", result);
            Assert.DoesNotContain("Additional Instructions:", result);
        }

        [Fact]
        public void UserPrompt_3_FullInput_CorrectWorkflow()
        {
            var records = TestDataFactory.GetRecords();
            var context = TestDataFactory.GetContexts(1);
            var entry = TestDataFactory.FullEntry();
            var query = "What is the main topic?";
            var result = _service.BuildUserPrompt(query, records, context, entry);

            // Check for presence of all sections using factory data keywords
            Assert.Contains("User Query:", result);
            Assert.Contains(query, result);
            Assert.Contains("Augmented data (context):", result);
            Assert.Contains(records.First().LLMRepresentationString, result);
            Assert.Contains(records.Last().LLMRepresentationString, result);
            Assert.Contains("Additional context for query:", result);
            Assert.Contains(context.First(), result);
            Assert.Contains("Original text (for query):", result);
            Assert.Contains(entry.EntryAdditionalData!, result);
            Assert.Contains("Additional data (for query):", result);
            Assert.Contains(entry.EntryOriginalText!, result);
        }

        [Fact]
        // UNIT TEST 4: User Prompt - Edge Case: Minimal Input and Null/Empty Collections
        public void UserPrompt_4_OnlyQuery_ExcludesAllOptionalSections()
        {
            var query = "Just a query.";
            var result = _service.BuildUserPrompt(query, Enumerable.Empty<IKnowledgeRecord>(), Enumerable.Empty<string>(), null);

            Assert.Contains("User Query:", result);
            Assert.Contains(query, result);
            // Check that optional sections are absent
            Assert.DoesNotContain("Augmented data (context):", result);
            Assert.DoesNotContain("Additional context for query:", result);
            Assert.DoesNotContain("Original text (for query):", result);
        }

        [Fact]
        public void Prompt_5_RegexReplacement_CollapsesNewlinesAndWhitespaces()
        {
            // Use a badly formatted query from the factory
            var rawInput = TestDataFactory.BadlyFormattedQuery;

            var result = _service.BuildUserPrompt(rawInput, Enumerable.Empty<IKnowledgeRecord>(), Enumerable.Empty<string>(), null);

            // Critical Assertions for the regex functionality
            Assert.DoesNotContain("  ", result);
            Assert.DoesNotContain("\n\n", result);
        }
    }
}
