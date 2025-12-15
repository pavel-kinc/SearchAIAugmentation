using ErrorOr;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Moq;
using PromptEnhancer.KernelServiceTemplates;
using PromptEnhancer.Models.Configurations;
using PromptEnhancer.Models.Enums;
using PromptEnhancer.Plugins.Interfaces;
using PromptEnhancer.SK.Interfaces;
using PromptEnhancer.Tests.TestDataFactory;

namespace PromptEnhancer.SK.Tests
{

    public class SemanticKernelManagerTests
    {
        private readonly Mock<IKernelServiceFactory> _mockFactory = new();
        private readonly Mock<ILogger<SemanticKernelManager>> _mockLogger = new();
        private readonly Mock<DummyPlugin> _mockContextPlugin = new();
        private readonly SemanticKernelManager _manager;
        private readonly Kernel _mockKernel = KernelMocks.GetRealKernelWithMocks();

        private readonly IEnumerable<ISemanticKernelContextPlugin> _contextPlugins;

        public SemanticKernelManagerTests()
        {
            _contextPlugins = new List<ISemanticKernelContextPlugin> { _mockContextPlugin.Object };

            // Initialize the manager with mocks
            _manager = new SemanticKernelManager(
                _mockFactory.Object,
                _contextPlugins,
                _mockLogger.Object
            );
        }


        [Fact(DisplayName = "AddPluginToSemanticKernel adds the correct plugin via type")]
        public void AddPluginToSemanticKernel_AddsPluginFromType()
        {
            var mockKernel = _mockKernel;

            Assert.False(mockKernel.Plugins.Any());
            _manager.AddPluginToSemanticKernel<DummyPlugin>(mockKernel);

            Assert.True(mockKernel.Plugins.Any());
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Adding plugin {nameof(DummyPlugin)} to Semantic Kernel.")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        // =========================================================================
        // --- 2. CreateKernel Tests ---
        // =========================================================================

        [Fact(DisplayName = "CreateKernel calls Factory and adds Context Plugins by default")]
        public void CreateKernel_Default_CallsFactoryAndAddsContextPlugins()
        {

            var configs = new List<KernelServiceBaseConfig> {
            new(AIProviderEnum.OpenAI, "model", "key")};
            var mockServices = new List<IKernelServiceTemplate>();

            _mockFactory.Setup(f => f.CreateKernelServicesConfig(configs))
                        .Returns(mockServices)
                        .Verifiable();


            var result = _manager.CreateKernel(configs);


            Assert.False(result.IsError);
            Assert.NotNull(result.Value);

            // Verify factory was called
            _mockFactory.Verify(f => f.CreateKernelServicesConfig(configs), Times.Once);
        }

        [Fact(DisplayName = "CreateKernel skips Context Plugins when addContextPlugins is false")]
        public void CreateKernel_SkipsContextPlugins()
        {

            var configs = new List<KernelServiceBaseConfig> {
            new(AIProviderEnum.OpenAI, "model", "key")};
            var mockServices = new List<IKernelServiceTemplate>();

            _mockFactory.Setup(f => f.CreateKernelServicesConfig(configs))
                        .Returns(mockServices);


            var result = _manager.CreateKernel(configs, addContextPlugins: false);


            Assert.False(result.IsError);
            Assert.NotNull(result.Value);
        }

        [Fact(DisplayName = "CreateKernel returns Error on Exception")]
        public void CreateKernel_ReturnsErrorOnException()
        {

            var configs = new List<KernelServiceBaseConfig> {
            new(AIProviderEnum.OpenAI, "model", "key")};

            // Simulate a failure in the factory call (or internal SK build process)
            _mockFactory.Setup(f => f.CreateKernelServicesConfig(It.IsAny<IEnumerable<KernelServiceBaseConfig>>()))
                        .Throws(new InvalidOperationException("Service setup failed."));


            var result = _manager.CreateKernel(configs);


            Assert.True(result.IsError);
            Assert.Equal(ErrorType.Failure, result.FirstError.Type);
            Assert.Contains("failed kernel creation", result.FirstError.Code);
        }

        // =========================================================================
        // --- 3. ConvertConfig Tests ---
        // =========================================================================

        [Fact(DisplayName = "ConvertConfig creates one LLM config when no embedding config is present")]
        public void ConvertConfig_OnlyLLM_CreatesOneConfig()
        {

            var kernelData = new KernelConfiguration
            {
                Provider = AIProviderEnum.AzureOpenAI,
                Model = "gpt-4",
                AIApiKey = "test-key",
                DeploymentName = "test-deploy",
                ClientServiceId = "llm-client"
            };


            var result = _manager.ConvertConfig(kernelData);


            Assert.False(result.IsError);
            var configs = result.Value.ToList();
            Assert.Single(configs);

            var llmConfig = configs.First();
            Assert.Equal(kernelData.Model, llmConfig.Model);
            Assert.Equal(kernelData.AIApiKey, llmConfig.Key);
            Assert.Equal(KernelServiceEnum.ChatClient, llmConfig.KernelServiceType);
        }

        [Fact(DisplayName = "ConvertConfig creates two configs, using LLM config for Embedding")]
        public void ConvertConfig_EmbeddingsUseLLMConfig_CreatesTwoConfigs()
        {

            var kernelData = new KernelConfiguration
            {
                Provider = AIProviderEnum.AzureOpenAI,
                Model = "gpt-4",
                AIApiKey = "test-key",
                EmbeddingModel = "text-embedding-ada-002",
                UseLLMConfigForEmbeddings = true,
                ClientServiceId = "llm-client",
                GeneratorServiceId = "emb-gen"
            };


            var result = _manager.ConvertConfig(kernelData);


            Assert.False(result.IsError);
            var configs = result.Value.ToList();
            Assert.Equal(2, configs.Count);

            var llmConfig = configs.First();
            Assert.Equal(kernelData.Provider, llmConfig.KernelServiceProvider);
            Assert.Equal(kernelData.AIApiKey, llmConfig.Key); // Should use LLM Key
            Assert.Equal(KernelServiceEnum.ChatClient, llmConfig.KernelServiceType);

            var embeddingConfig = configs.Last();
            Assert.Equal(kernelData.EmbeddingModel, embeddingConfig.Model);
            Assert.Equal(kernelData.Provider, embeddingConfig.KernelServiceProvider); // Should use LLM Provider
            Assert.Equal(kernelData.AIApiKey, embeddingConfig.Key); // Should use LLM Key
            Assert.Equal(KernelServiceEnum.EmbeddingGenerator, embeddingConfig.KernelServiceType);
        }

        [Fact(DisplayName = "ConvertConfig creates two separate configs for LLM and Embedding")]
        public void ConvertConfig_EmbeddingsUseSeparateConfig_CreatesTwoConfigs()
        {

            var kernelData = new KernelConfiguration
            {
                Provider = AIProviderEnum.OpenAI,
                Model = "gpt-4",
                AIApiKey = "llm-key",
                EmbeddingModel = "text-embedding-ada-002",
                UseLLMConfigForEmbeddings = false,
                EmbeddingProvider = AIProviderEnum.AzureOpenAI,
                EmbeddingKey = "emb-key",
                ClientServiceId = "llm-client",
                GeneratorServiceId = "emb-gen"
            };


            var result = _manager.ConvertConfig(kernelData);


            Assert.False(result.IsError);
            var configs = result.Value.ToList();
            Assert.Equal(2, configs.Count);

            var llmConfig = configs.First();
            Assert.Equal(AIProviderEnum.OpenAI, llmConfig.KernelServiceProvider);
            Assert.Equal("llm-key", llmConfig.Key);
            Assert.Equal(KernelServiceEnum.ChatClient, llmConfig.KernelServiceType);

            var embeddingConfig = configs.Last();
            Assert.Equal(kernelData.EmbeddingModel, embeddingConfig.Model);
            Assert.Equal(AIProviderEnum.AzureOpenAI, embeddingConfig.KernelServiceProvider); // Verify separate provider
            Assert.Equal("emb-key", embeddingConfig.Key); // Verify separate key
            Assert.Equal(KernelServiceEnum.EmbeddingGenerator, embeddingConfig.KernelServiceType);
        }

        [Fact(DisplayName = "ConvertConfig returns Error on Exception")]
        public void ConvertConfig_ReturnsErrorOnException()
        {
            // Simulate invalid data that might cause an error
            var kernelData = new KernelConfiguration
            {
                Provider = AIProviderEnum.AzureOpenAI,
                Model = null!, // Intentionally null to test
                AIApiKey = "test-key"
            };

            var result = _manager.ConvertConfig(kernelData);


            Assert.True(result.IsError);
            Assert.Equal(ErrorType.Failure, result.FirstError.Type);
            Assert.Contains("Failed to convert KernelConfiguration", _mockLogger.Invocations.Last().Arguments[2].ToString());
            Assert.Contains("failed", result.FirstError.Code);
        }
    }
    public class DummyPlugin : ISemanticKernelContextPlugin
    {
        [KernelFunction("")]
        public void DoNothing()
        {

        }
    }
}
