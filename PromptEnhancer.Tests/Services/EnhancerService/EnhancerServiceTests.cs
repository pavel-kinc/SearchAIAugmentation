using ErrorOr;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Moq;
using Newtonsoft.Json;
using PromptEnhancer.KnowledgeBaseCore;
using PromptEnhancer.KnowledgeBaseCore.Examples;
using PromptEnhancer.KnowledgeBaseCore.Interfaces;
using PromptEnhancer.KnowledgeRecord;
using PromptEnhancer.KnowledgeSearchRequest.Examples;
using PromptEnhancer.Models;
using PromptEnhancer.Models.Configurations;
using PromptEnhancer.Models.Enums;
using PromptEnhancer.Models.Pipeline;
using PromptEnhancer.Pipeline.Interfaces;
using PromptEnhancer.Pipeline.PromptEnhancerSteps;
using PromptEnhancer.Search.Interfaces;
using PromptEnhancer.Services.ChatHistoryService;
using PromptEnhancer.Services.ChunkService;
using PromptEnhancer.SK.Interfaces;
using PromptEnhancer.Tests.TestClasses;
using PromptEnhancer.Tests.TestDataFactory;
using System.Text;

namespace PromptEnhancer.Services.EnhancerService.Tests
{
    public class EnhancerServiceTests
    {
        private readonly Mock<ISemanticKernelManager> _mockSKManager = new();
        private readonly Mock<IPipelineOrchestrator> _mockOrchestrator = new();
        private readonly Mock<ILogger<EnhancerService>> _mockLogger = new();
        private readonly Mock<IServiceProvider> _mockServiceProvider = new();

        private readonly Mock<ISearchWebScraper> _mockScraper = new();
        private readonly Mock<IChunkGeneratorService> _mockChunkGenerator = new();
        private readonly Mock<ISearchProviderManager> _mockSearchProviderManager = new();
        private readonly Mock<ILogger<GoogleKnowledgeBase>> _mockGoogleKBLogger = new();
        private readonly Mock<IChatHistoryService> _mockChatHistoryService = new();
        private readonly Mock<GoogleKnowledgeBase> _mockGoogleKB;
        private readonly Kernel _mockKernel = KernelMocks.GetRealKernelWithMocks();
        private readonly EnhancerService _service;

        public EnhancerServiceTests()
        {
            _mockGoogleKB = new Mock<GoogleKnowledgeBase>(
            _mockScraper.Object,
            _mockChunkGenerator.Object,
            _mockSearchProviderManager.Object,
            _mockGoogleKBLogger.Object
            );

            _service = new EnhancerService(
                _mockSKManager.Object,
                _mockOrchestrator.Object,
                _mockServiceProvider.Object,
                _mockGoogleKB.Object,
                _mockLogger.Object,
                _mockChatHistoryService.Object
            );

        }

        // Since file operations are involved, use a temporary file path
        private const string TestFilePath = "test_config_temp.json";

        #region ConfigurationManagementTests

        [Theory]
        [InlineData(null, AIProviderEnum.OpenAI, "gpt-3.5-turbo", "text-embedding-ada-002", true)]
        [InlineData("test_key", AIProviderEnum.AzureOpenAI, "azure-model", null, false)]
        [InlineData("test_key", AIProviderEnum.GoogleGemini, "gemini-2.5-flash", "text-embedding-004", false)]
        public void Config_1_Theory_CreateDefaultConfiguration_SetsAllFieldsCorrectly(
            string? apiKey, AIProviderEnum provider, string model, string? embeddingModel, bool useLLMConfig)
        {
            var config = _service.CreateDefaultConfiguration(apiKey, provider, model, embeddingModel);

            Assert.NotNull(config);
            Assert.NotNull(config.KernelConfiguration);
            Assert.Equal(apiKey, config.KernelConfiguration.AIApiKey);
            Assert.Equal(provider, config.KernelConfiguration.Provider);
            Assert.Equal(model, config.KernelConfiguration.Model);
            Assert.Equal(embeddingModel, config.KernelConfiguration.EmbeddingModel);
            Assert.Equal(useLLMConfig, config.KernelConfiguration.UseLLMConfigForEmbeddings);
        }

        [Fact]
        public void Config_2_ExportConfigurationToBytes_HidesSecrets()
        {
            var config = new EnhancerConfiguration
            {
                KernelConfiguration = new KernelConfiguration { AIApiKey = "SUPER_SECRET_KEY" }
            };

            var bytes = _service.ExportConfigurationToBytes(config, hideSecrets: true);
            var json = Encoding.UTF8.GetString(bytes);

            // SensitiveContractResolver should mask the key
            Assert.DoesNotContain("SUPER_SECRET_KEY", json);
        }

        [Fact]
        public void Config_3_ExportConfigurationToBytes_IncludesSecrets()
        {
            var config = new EnhancerConfiguration
            {
                KernelConfiguration = new KernelConfiguration { AIApiKey = "SUPER_SECRET_KEY" }
            };

            var bytes = _service.ExportConfigurationToBytes(config, hideSecrets: false);
            var json = Encoding.UTF8.GetString(bytes);

            Assert.Contains("SUPER_SECRET_KEY", json);
        }

        [Fact]
        public async Task Config_4_DownloadConfiguration_WritesToFile()
        {
            if (File.Exists(TestFilePath)) File.Delete(TestFilePath);

            var config = new EnhancerConfiguration();
            await _service.DownloadConfiguration(config, TestFilePath, hideSecrets: false);

            Assert.True(File.Exists(TestFilePath));
            var content = await File.ReadAllTextAsync(TestFilePath);
            Assert.NotEmpty(content);

            File.Delete(TestFilePath);
        }

        [Fact]
        public void Config_5_ImportConfigurationFromBytes_Succeeds()
        {
            var originalConfig = new EnhancerConfiguration { PipelineAdditionalSettings = new PipelineAdditionalSettings { MaximumInputLength = 1000 } };
            var json = JsonConvert.SerializeObject(originalConfig);
            var bytes = Encoding.UTF8.GetBytes(json);

            var importedConfig = _service.ImportConfigurationFromBytes(bytes);

            Assert.NotNull(importedConfig);
            Assert.Equal(1000, importedConfig.PipelineAdditionalSettings.MaximumInputLength);
        }

        #endregion

        #region PipelineSetupTests

        [Fact]
        public void Setup_1_CreatePipelineSettingsFromConfig_UsesExistingKernel()
        {
            var promptConf = new PromptConfiguration();
            var pipelineSettings = new PipelineAdditionalSettings();

            var result = _service.CreatePipelineSettingsFromConfig(promptConf, pipelineSettings, kernelData: null, kernel: _mockKernel);

            Assert.False(result.IsError);
            Assert.Equal(_mockKernel, result.Value.Kernel);
        }


        [Fact]
        public void Setup_2_CreatePipelineSettingsFromConfig_FailsIfNoKernelAvailable()
        {
            var result = _service.CreatePipelineSettingsFromConfig(new PromptConfiguration(), new PipelineAdditionalSettings(), kernelData: null, kernel: null);

            Assert.True(result.IsError);
            Assert.Equal("No kernel could be created or resolved.", result.Errors.First().Code);
        }

        #endregion

        #region PipelineExecutionTests

        [Fact]
        public async Task Execution_1_ExecutePipelineAsync_RunsParallelAndCollectsResults()
        {
            var pipeline = TestDataFactory.GetPipelineModel();
            var entries = new List<PipelineRun>();
            var successfulRunContextA = new PipelineRun(new Entry { QueryString = "A" });
            var successfulRunContextB = new PipelineRun(new Entry { QueryString = "B" }) { FinalResponse = new ChatResponse(new ChatMessage(ChatRole.Assistant, "Result B")) };
            successfulRunContextA.FinalResponse = new ChatResponse(new ChatMessage(ChatRole.Assistant, "Result A"));
            entries.Add(successfulRunContextA);
            entries.Add(successfulRunContextB);

            // Setup orchestrator to return success for both entries
            _mockOrchestrator
                .Setup(o => o.RunPipelineAsync(pipeline, entries[0], It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            _mockOrchestrator
                .Setup(o => o.RunPipelineAsync(pipeline, entries[1], It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await _service.ExecutePipelineAsync(pipeline, entries);

            Assert.False(result.IsError);
            Assert.Equal(2, result.Value.Count);
            Assert.Contains(result.Value, r => r.Result!.FinalResponse!.Text.Contains("Result A"));
            Assert.Contains(result.Value, r => r.Result!.FinalResponse!.Text.Contains("Result B"));
        }

        [Fact]
        public async Task Execution_2_ExecutePipelineAsync_HandlesPipelineFailure()
        {
            var pipeline = TestDataFactory.GetPipelineModel();
            var entries = new List<PipelineRun> { new(new Entry { QueryString = "A" }) };
            var error = Error.Failure("Step failed");

            // Setup orchestrator to return error
            _mockOrchestrator
                .Setup(o => o.RunPipelineAsync(pipeline, entries.First(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(error);

            var result = await _service.ExecutePipelineAsync(pipeline, entries);

            Assert.False(result.IsError);
            Assert.Single(result.Value);
            Assert.True(result.Value.First().Errors.Any());
            Assert.Equal(error.Description, result.Value.First().Errors.First().Description);
        }

        [Fact]
        public async Task Execution_3_ProcessConfiguration_ReturnsErrorOnSettingsFailure()
        {
            var config = new EnhancerConfiguration();
            var entry = new Entry { QueryString = "test" };
            var error = Error.Failure("Settings creation failed");

            // Mock failure in CreatePipelineSettingsFromConfig
            _mockSKManager.Setup(m => m.ConvertConfig(It.IsAny<KernelConfiguration>())).Returns(error);

            var result = await _service.ProcessConfiguration(config, [entry]);

            Assert.True(result.IsError);
            Assert.Equal(error.Description, result.Errors.First().Description);
        }

        [Fact]
        public async Task Execution_4_ProcessConfiguration_SingleEntry_ReturnsFirstResult()
        {
            var config = new EnhancerConfiguration
            {
                PipelineAdditionalSettings = new PipelineAdditionalSettings(),
                PromptConfiguration = new PromptConfiguration(),
                Steps = []
            };
            var entry = new Entry { QueryString = "single entry" };
            var kernel = _mockKernel;


            // Setup orchestrator
            _mockOrchestrator
                .Setup(o => o.RunPipelineAsync(It.IsAny<PipelineModel>(), It.IsAny<PipelineRun>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Use the ProcessConfiguration overload that takes a single Entry
            var result = await _service.ProcessConfiguration(config, entry, kernel);

            Assert.False(result.IsError);
        }

        #endregion


        #region StreamingResponseTests

        [Fact]
        public async Task Streaming_1_GetStreamingResponse_CallsChatClientWithHistory()
        {
            _mockChatHistoryService
                .Setup(s => s.CreateChatHistoryFromPipelineRun(It.IsAny<PipelineRun>()))
                .Returns(new List<ChatMessage> { new ChatMessage(ChatRole.Assistant, "chunk") });

            _mockChatHistoryService
                .Setup(s => s.GetHistoryLength(It.IsAny<List<ChatMessage>>()))
                .Returns((List<ChatMessage> history) => 5);

            var chatClientMock = new Mock<IChatClient>();
            var chatUpdates = new List<ChatResponseUpdate> { new(ChatRole.Assistant, "chunk") }.ToAsyncEnumerable();
            chatClientMock.Setup(c => c.GetStreamingResponseAsync(
                    It.IsAny<List<ChatMessage>>(),
                    It.IsAny<ChatOptions>(),
                    It.IsAny<CancellationToken>()))
                .Returns(chatUpdates);
            var kernelMock = KernelMocks.GetRealKernelWithMocks(chatClient: chatClientMock);

            var settings = new PipelineSettings(kernelMock, _mockServiceProvider.Object, new PipelineAdditionalSettings(), new PromptConfiguration());
            var context = new PipelineRun(new Entry { QueryString = "User query" });

            var result = _service.GetStreamingResponse(settings, context);
            var firstChunk = await result.FirstAsync();

            Assert.Equal("chunk", firstChunk.Text);
            Assert.True(context.ChatHistory!.Any());
        }

        [Fact]
        public void Streaming_2_GetStreamingResponse_ThrowsIfInputSizeExceeded()
        {
            _mockChatHistoryService
                .Setup(s => s.GetHistoryLength(It.IsAny<List<ChatMessage>>()))
                .Returns((List<ChatMessage> history) => 20);
            var chatClientMock = new Mock<IChatClient>();
            var kernelMock = KernelMocks.GetRealKernelWithMocks(chatClient: chatClientMock);
            var settings = new PipelineSettings(kernelMock, _mockServiceProvider.Object, new PipelineAdditionalSettings { MaximumInputLength = 10 }, new PromptConfiguration());

            Assert.Throws<ArgumentOutOfRangeException>(() => _service.GetStreamingResponse(settings, new PipelineRun()));
        }

        #endregion

        // --- 5. Knowledge Base/Container Management Tests ---

        #region KnowledgeBaseContainerTests

        [Fact]
        public void KB_1_CreateDefaultDataContainer_Generic_CreatesCorrectContainerType()
        {
            var data = new List<string> { };

            var container = _service.CreateDefaultDataContainer(data);

            Assert.NotNull(container);
            // Using Contains check as type names can be assembly-qualified and long
            Assert.Contains("1[[System.String", container!.GetType().FullName!);

        }

        [Fact]
        public void KB_2_CreateDefaultDataContainer_SpecificRecord_CreatesCorrectContainerType()
        {
            var data = new List<DummyKnowledgeRecord> { };

            // Test with a specific, mocked record type
            var container = _service.CreateDefaultDataContainer(data);

            Assert.NotNull(container);
            // The container should wrap a KnowledgeBaseDataDefault<MockIKnowledgeRecord, TestModel>
            Assert.Contains("DummyKnowledgeRecord", container.GetType().FullName!);
        }

        [Fact]
        public void KB_3_CreateContainer_CreatesCorrectContainerType()
        {
            var kb = _mockGoogleKB.Object;
            var request = new GoogleSearchRequest() { Settings = new GoogleSettings() { SearchApiKey = "key", Engine = "qwe" } };
            var filter = new UrlRecordFilter();

            var container = _service.CreateContainer(kb, request, filter);

            Assert.NotNull(container);
            // Verify the generic container type is correct
            Assert.Contains("UrlRecordFilter", container.GetType().FullName);
            Assert.Contains("GoogleSettings", container.GetType().FullName);
            Assert.Contains("KnowledgeUrlRecord", container.GetType().FullName);
            Assert.Contains("GoogleSearchFilterModel", container.GetType().FullName);
            Assert.Contains("UrlRecord", container.GetType().FullName);
        }

        [Fact]
        public void KB_4_CreateDefaultSearchPipelineSteps_IncludesGenerationStep()
        {
            var containers = new List<IKnowledgeBaseContainer> { Mock.Of<IKnowledgeBaseContainer>() };

            var steps = _service.CreateDefaultSearchPipelineSteps(containers).ToList();

            // Should contain 8 steps, including the final GenerationStep
            Assert.Equal(8, steps.Count);
            Assert.IsType<GenerationStep>(steps.Last());
        }

        [Fact]
        public void KB_5_CreateDefaultSearchPipelineStepsWithoutGenerationStep_ExcludesGenerationStep()
        {
            var containers = new List<IKnowledgeBaseContainer> { Mock.Of<IKnowledgeBaseContainer>() };

            var steps = _service.CreateDefaultSearchPipelineStepsWithoutGenerationStep(containers).ToList();

            // Should contain 7 steps
            Assert.Equal(7, steps.Count);
            Assert.IsNotType<GenerationStep>(steps.Last());
            Assert.IsType<PromptBuilderStep>(steps.Last());
        }

        [Fact]
        public void KB_6_CreateDefaultGoogleSearchPipelineSteps_CreatesExpectedSteps()
        {
            // Simple case: default settings
            var steps = _service.CreateDefaultGoogleSearchPipelineSteps("test_key", "test_engine").ToList();

            Assert.Equal(9, steps.Count);
            Assert.IsType<GenerationStep>(steps.Last());
            Assert.IsType<KernelContextPluginsStep>(steps.Skip(1).First());
        }

        [Fact]
        public void KB_7_CreateDefaultGoogleSearchPipelineSteps_UsesPassedSettings()
        {
            // Custom case: ensure custom filter and settings are used in container creation
            var steps = _service.CreateDefaultGoogleSearchPipelineSteps(
                "test_key",
                "test_engine",
                searchFilter: new GoogleSearchFilterModel { Top = 20 },
                useScraper: true
            ).ToList();

            Assert.IsType<GenerationStep>(steps.Last());
        }

        #endregion
    }
}
