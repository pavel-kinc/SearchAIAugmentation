using Microsoft.Extensions.Logging;
using Moq;
using PromptEnhancer.KernelServiceTemplates.ChatClients;
using PromptEnhancer.KernelServiceTemplates.EmbeddingGenerators;
using PromptEnhancer.Models.Configurations;
using PromptEnhancer.Models.Enums;

namespace PromptEnhancer.SK.Tests
{
    public class KernelServiceFactoryTests
    {
        private readonly Mock<ILogger<KernelServiceFactory>> _mockLogger = new();
        private readonly KernelServiceFactory _factory;

        public KernelServiceFactoryTests()
        {
            // Initialize the factory with the mocked logger
            _factory = new KernelServiceFactory(_mockLogger.Object);
        }

        [Fact(DisplayName = "Factory returns an empty list for empty input config")]
        public void CreateKernelServicesConfig_EmptyConfigs_ReturnsEmpty()
        {

            var configs = Enumerable.Empty<KernelServiceBaseConfig>();


            var result = _factory.CreateKernelServicesConfig(configs);


            Assert.Empty(result);

            // Verify the summary log (1 call)
            _mockLogger.Verify(
                x => x.Log(LogLevel.Information, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Theory(DisplayName = "Successfully creates service for each supported provider/type")]
        [InlineData(AIProviderEnum.OpenAI, KernelServiceEnum.ChatClient, typeof(OpenAIChatClient))]
        [InlineData(AIProviderEnum.OpenAI, KernelServiceEnum.EmbeddingGenerator, typeof(OpenAIEmbeddingGenerator))]
        [InlineData(AIProviderEnum.AzureOpenAI, KernelServiceEnum.ChatClient, typeof(AzureOpenAIChatClient))]
        [InlineData(AIProviderEnum.AzureOpenAI, KernelServiceEnum.EmbeddingGenerator, typeof(AzureOpenAIEmbeddingGenerator))]
        [InlineData(AIProviderEnum.GoogleGemini, KernelServiceEnum.ChatClient, typeof(GeminiChatClient))]
        [InlineData(AIProviderEnum.GoogleGemini, KernelServiceEnum.EmbeddingGenerator, typeof(GeminiEmbeddingGenerator))]
        [InlineData(AIProviderEnum.Ollama, KernelServiceEnum.ChatClient, typeof(OllamaChatClient))]
        [InlineData(AIProviderEnum.Ollama, KernelServiceEnum.EmbeddingGenerator, typeof(OllamaEmbeddingGenerator))]
        public void CreateKernelServicesConfig_ValidConfig_CreatesCorrectServiceType(
            AIProviderEnum provider, KernelServiceEnum serviceType, Type expectedType)
        {

            var configs = new List<KernelServiceBaseConfig>
            {
                // Key must be valid for the mapping to succeed
                new(provider, "test-model", "test-key", "test-deploy", serviceType: serviceType)
            };


            var result = _factory.CreateKernelServicesConfig(configs).ToList();


            Assert.Single(result);
            Assert.IsType(expectedType, result.First());

            // Verify Information logging (1 for successful creation + 1 for summary)
            _mockLogger.Verify(
                x => x.Log(LogLevel.Information, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Exactly(2));
        }

        [Fact(DisplayName = "Factory successfully creates a mix of services")]
        public void CreateKernelServicesConfig_MixedValidConfigs_CreatesMultipleServices()
        {

            var configs = new List<KernelServiceBaseConfig>
            {
                new(AIProviderEnum.OpenAI, "gpt", "key1", serviceType: KernelServiceEnum.ChatClient),
                new(AIProviderEnum.AzureOpenAI, "embed", "key2", "deploy", serviceType: KernelServiceEnum.EmbeddingGenerator)
            };


            var result = _factory.CreateKernelServicesConfig(configs).ToList();


            Assert.Equal(2, result.Count);
            Assert.IsType<OpenAIChatClient>(result.First());
            Assert.IsType<AzureOpenAIEmbeddingGenerator>(result.Last());

            // Verify Information logging (2 for successful creations + 1 for summary)
            _mockLogger.Verify(
                x => x.Log(LogLevel.Information, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Exactly(3));
        }


        [Fact(DisplayName = "Factory skips creation if API Key is null/whitespace and logs a warning")]
        public void AddToServiceTemplatesIfValid_MissingKey_SkipsAndLogsWarning()
        {

            var configs = new List<KernelServiceBaseConfig>
            {
                new(AIProviderEnum.OpenAI, "model", null!, serviceType: KernelServiceEnum.ChatClient), // Key is null
                new(AIProviderEnum.AzureOpenAI, "model", " ", "deploy", serviceType: KernelServiceEnum.EmbeddingGenerator) // Key is whitespace
            };


            var result = _factory.CreateKernelServicesConfig(configs).ToList();


            Assert.Empty(result);

            // Verify warning logging (2 calls for the two skipped configs)
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("No factory found for provider:") && v.ToString().Contains("API key is missing.")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Exactly(2));
        }

        [Fact(DisplayName = "Factory skips creation if provider/type combination is not supported and logs a warning")]
        public void AddToServiceTemplatesIfValid_UnsupportedCombination_SkipsAndLogsWarning()
        {

            // Assuming a non-standard Enum value that won't exist in the static Factories dictionary
            var unsupportedProvider = (AIProviderEnum)99;

            var configs = new List<KernelServiceBaseConfig>
            {
                new(unsupportedProvider, "model", "test-key", serviceType: KernelServiceEnum.ChatClient)
            };


            var result = _factory.CreateKernelServicesConfig(configs).ToList();


            Assert.Empty(result);

            // Verify warning logging (1 call for the unsupported config)
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("No factory found for provider:") && v.ToString().Contains("API key is missing.")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
