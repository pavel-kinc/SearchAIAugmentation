using Microsoft.SemanticKernel;
using PromptEnhancer.KnowledgeRecord.Interfaces;
using PromptEnhancer.Models;
using PromptEnhancer.Models.Configurations;
using PromptEnhancer.Models.Enums;
using PromptEnhancer.Models.Pipeline;
using PromptEnhancer.Tests.TestClasses;

namespace PromptEnhancer.Tests.TestDataFactory
{
    public static class TestDataFactory
    {
        /// <summary>
        /// Returns a fully populated PromptConfiguration instance (for happy path tests).
        /// </summary>
        public static PromptConfiguration FullPromptConfiguration() => new()
        {
            SystemInstructions = "You are an expert technical writer.",
            TargetOutputLength = 1000,
            MacroDefinition = "##",
            AdditionalInstructions = "Be precise and use Markdown formatting.",
            TargetLanguageCultureCode = "en-US"
        };

        /// <summary>
        /// Returns a minimal PromptConfiguration instance (for default/null case tests).
        /// </summary>
        public static PromptConfiguration MinimalPromptConfiguration() => new()
        {
            SystemInstructions = "Default instructions.",
            TargetOutputLength = 500,
            MacroDefinition = null,
            AdditionalInstructions = null,
            TargetLanguageCultureCode = "de-DE"
        };

        /// <summary>
        /// Returns a PromptConfiguration with problematic formatting (for regex tests).
        /// </summary>
        public static PromptConfiguration BadlyFormattedConfiguration() => new()
        {
            SystemInstructions = "Start\n\nInstructions\t\tEnd",
            TargetOutputLength = 100,
            TargetLanguageCultureCode = "fr-FR"
        };

        /// <summary>
        /// Returns a list of Knowledge Records for testing.
        /// </summary>
        public static IEnumerable<IKnowledgeRecord> GetRecords()
        {
            return
                [
                    new DummyKnowledgeRecord { SourceObject = "Some data", Source = "Some source", UsedSearchQuery = "query"},
                    new DummyKnowledgeRecord { SourceObject = "Additional data", Source = "Some source", UsedSearchQuery = "other query"},
                    new DummyKnowledgeRecord { SourceObject = "More data", Source = "Another source", UsedSearchQuery = "query"}
                ];
        }

        public static IEnumerable<IKnowledgeRecord> GetTestRecords() =>
        [
            new DummyKnowledgeRecord { SimilarityScore = 0.8, Embeddings = EmbeddingMocksUtilities.CreateEmbeddings("AlphaSource"), Source = "Alpha", SourceObject = "dummy1" },
            new DummyKnowledgeRecord { SimilarityScore = 0.9, Embeddings = EmbeddingMocksUtilities.CreateEmbeddings("BetaSource"), Source = "Beta" , SourceObject = "dummy2"},
            new DummyKnowledgeRecord { SimilarityScore = 0.6, Embeddings = EmbeddingMocksUtilities.CreateEmbeddings("AlphaSource"), Source = "Alpha" , SourceObject = "dummy3"},
            new DummyKnowledgeRecord { SimilarityScore = null, Embeddings = EmbeddingMocksUtilities.CreateEmbeddings("GammaSource"), Source = "Gamma" , SourceObject = "dummy4"}, // Score is null
            new DummyKnowledgeRecord { SimilarityScore = 0.7, Embeddings = null, Source = "Delta" } // Embeddings is null
        ];

        public static PipelineModel GetPipelineModel()
        {
            var settings = new PipelineSettings(new Kernel(), null, new PipelineAdditionalSettings(), new PromptConfiguration());
            return new PipelineModel(settings, []);
        }

        /// <summary>
        /// Returns a list of Additional Context strings for testing.
        /// </summary>
        public static IEnumerable<string> GetContexts(int count = 1)
        {
            return Enumerable.Range(1, count)
                .Select(i => $"Context Item {i}.");
        }

        /// <summary>
        /// Returns a fully populated Entry model.
        /// </summary>
        public static Entry FullEntry() => new()
        {
            QueryString = "What is the main topic?",
            EntryName = "Entry name",
            EntryOriginalText = "The original document text.",
            EntryAdditionalData = "Metadata from the source system."
        };

        /// <summary>
        /// Returns an Entry model with only the original text set.
        /// </summary>
        public static Entry PartialEntry() => new()
        {
            QueryString = "What is the main topic?",
            EntryOriginalText = "Only the original text is here.",
        };

        public static IEnumerable<KernelServiceBaseConfig> GetDefaultKernelConfigs()
        {
            // This method returns the actual configuration data used in production
            return new List<KernelServiceBaseConfig>
            {
                // Example: A valid OpenAI chat client config
                new(AIProviderEnum.OpenAI, "gpt-4", "test-openai-key", serviceId: "openai", serviceType: KernelServiceEnum.ChatClient),
                // Example: An Azure embedding generator config
                new(AIProviderEnum.AzureOpenAI, "text-embedding", "test-azure-key", "emb-deploy", "azure", KernelServiceEnum.EmbeddingGenerator)
            };
        }

        /// <summary>
        /// Returns a query string with bad internal formatting (for regex tests).
        /// </summary>
        public const string BadlyFormattedQuery = "What \n\n is the \t\t primary function?";

        /// <summary>
        /// Returns the expected cleaned version of the badly formatted query.
        /// </summary>
        public const string CleanedQuery = "What is the primary function?";
    }
}
