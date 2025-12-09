using ErrorOr;
using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel;
using PromptEnhancer.KnowledgeBaseCore;
using PromptEnhancer.KnowledgeBaseCore.Interfaces;
using PromptEnhancer.KnowledgeRecord;
using PromptEnhancer.KnowledgeRecord.Interfaces;
using PromptEnhancer.KnowledgeSearchRequest.Examples;
using PromptEnhancer.KnowledgeSearchRequest.Interfaces;
using PromptEnhancer.Models;
using PromptEnhancer.Models.Configurations;
using PromptEnhancer.Models.Enums;
using PromptEnhancer.Models.Pipeline;
using PromptEnhancer.Pipeline.Interfaces;

namespace PromptEnhancer.Services.EnhancerService
{
    /// <summary>
    /// Defines a main entry point service for the PromptEnhancer library - creating, managing, and processing configurations, pipelines, and data containers  for
    /// AI-enhanced workflows. Provides methods for handling configurations, pipelines, and streaming responses,  as
    /// well as utilities for importing, exporting, and managing data.
    /// </summary>
    /// <remarks>This interface is designed to support a variety of AI-related operations, including
    /// configuration management,  pipeline creation and execution, and data container management. It includes methods
    /// for both synchronous and  asynchronous operations, as well as utilities for importing and exporting
    /// configurations. The service is  intended to be flexible and extensible, supporting multiple AI providers and
    /// customizable settings.</remarks>
    public interface IEnhancerService
    {
        /// <summary>
        /// Processes the settings and executes the pipeline with the provided configuration and entries to generate a list of pipeline results.
        /// </summary>
        /// <param name="config">The configuration settings used to enhance the processing logic.</param>
        /// <param name="entries">A collection of entries to be processed.</param>
        /// <param name="kernel">An optional kernel instance to customize the processing behavior. If null, a default kernel is used.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains an <see cref="ErrorOr{T}"/>
        /// object that either holds a list of <see cref="PipelineResultModel"/> instances if the operation succeeds, or an
        /// error if the operation fails.</returns>
        public Task<ErrorOr<IList<PipelineResultModel>>> ProcessConfiguration(EnhancerConfiguration config, IEnumerable<Entry> entries, Kernel? kernel = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Executes the specified pipeline asynchronously and processes the provided entries.
        /// </summary>
        /// <remarks>This method processes the provided entries using the specified pipeline
        /// configuration.  If the operation is canceled via the <paramref name="cancellationToken"/>, the task will 
        /// complete in a canceled state.</remarks>
        /// <param name="pipeline">The pipeline configuration to execute.</param>
        /// <param name="entries">A collection of pipeline run entries to be processed.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains an  <see cref="ErrorOr{T}"/>
        /// object that either holds a list of <see cref="PipelineResultModel"/>  representing the results of the
        /// pipeline execution, or an error indicating the failure.</returns>
        public Task<ErrorOr<IList<PipelineResultModel>>> ExecutePipelineAsync(PipelineModel pipeline, IEnumerable<PipelineRun> entries, CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a new instance of <see cref="PipelineSettings"/> based on the provided configuration and optional
        /// kernel data.
        /// </summary>
        /// <remarks>This method validates the provided configurations and combines them to produce a
        /// fully defined  <see cref="PipelineSettings"/> instance. If any required configuration is invalid, an error
        /// is returned.</remarks>
        /// <param name="promptConf">The prompt configuration used to define the pipeline's behavior and parameters. Cannot be null.</param>
        /// <param name="pipelineSettings">Additional settings to customize the pipeline. Cannot be null.</param>
        /// <param name="kernelData">Optional kernel configuration data to further refine the pipeline settings. Can be null.</param>
        /// <param name="kernel">An optional kernel instance to associate with the pipeline. Can be null.</param>
        /// <returns>An <see cref="ErrorOr{T}"/> containing the created <see cref="PipelineSettings"/> if successful,  or an
        /// error if the configuration is invalid.</returns>
        public ErrorOr<PipelineSettings> CreatePipelineSettingsFromConfig(PromptConfiguration promptConf, PipelineAdditionalSettings pipelineSettings, KernelConfiguration? kernelData = null, Kernel? kernel = null);

        /// <summary>
        /// Retrieves a streaming response as an asynchronous sequence of <see cref="ChatResponseUpdate"/> objects.
        /// </summary>
        /// <remarks>This method streams updates to the chat response as they become available. It is
        /// suitable for scenarios where  incremental updates are required, such as real-time chat applications. The
        /// caller can enumerate the returned  sequence asynchronously to process each update as it is
        /// received.</remarks>
        /// <param name="settings">The pipeline settings that configure the behavior of the response generation.</param>
        /// <param name="context">The context for the pipeline run, providing necessary state and data for the operation.</param>
        /// <param name="ct">An optional <see cref="CancellationToken"/> to observe while waiting for the operation to complete.</param>
        /// <returns>An <see cref="IAsyncEnumerable{T}"/> that yields <see cref="ChatResponseUpdate"/> objects representing
        /// updates to the chat response.</returns>
        public IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponse(PipelineSettings settings, PipelineRun context, CancellationToken ct = default);

        /// <summary>
        /// Creates a default configuration for the AI enhancer with the specified parameters.
        /// </summary>
        /// <remarks>This method provides a convenient way to create a configuration with sensible
        /// defaults,  while allowing customization of the AI provider, AI model, and embedding model.</remarks>
        /// <param name="aiApiKey">The API key used to authenticate with the AI provider. This parameter is optional and can be <see
        /// langword="null"/>.</param>
        /// <param name="aiProvider">The AI provider to use for the configuration. Defaults to <see cref="AIProviderEnum.OpenAI"/>.</param>
        /// <param name="aiModel">The AI model to use for processing. Defaults to "gpt-4o-mini".</param>
        /// <param name="embeddingModel">The embedding model to use for generating embeddings. Defaults to "text-embedding-3-small".</param>
        /// <returns>An instance of <see cref="EnhancerConfiguration"/> initialized with the specified parameters.</returns>
        public EnhancerConfiguration CreateDefaultConfiguration(string? aiApiKey = null, AIProviderEnum aiProvider = AIProviderEnum.OpenAI, string aiModel = "gpt-4o-mini", string embeddingModel = "text-embedding-3-small");

        /// <summary>
        /// Experimental! For better control over knowledge base, setup your own implementation, see example <see cref="KnowledgeBaseCore.Examples.GoogleKnowledgeBase"/> <br/>
        /// Creates a default data container for the specified model type.
        /// </summary>
        /// <typeparam name="TModel">The type of the model contained in the data. Must be a reference type.</typeparam>
        /// <param name="data">The collection of data items to be included in the container. Cannot be null.</param>
        /// <returns>An instance of <see cref="IKnowledgeBaseContainer"/> containing the provided data.</returns>
        public IKnowledgeBaseContainer CreateDefaultDataContainer<TModel>(IEnumerable<TModel> data) where TModel : class;

        /// <summary>
        /// Experimental! For better control over knowledge base, setup your own implementation, see example <see cref="KnowledgeBaseCore.Examples.GoogleKnowledgeBase"/> <br/>
        /// Creates a default data container for managing knowledge base records.
        /// </summary>
        /// <typeparam name="TRecord">The type of the knowledge record, which must inherit from <see cref="KnowledgeRecord{TModel}"/> and have a
        /// parameterless constructor.</typeparam>
        /// <typeparam name="TModel">The type of the model associated with the knowledge record. Must be a reference type.</typeparam>
        /// <param name="data">The collection of model instances to initialize the data container with. Cannot be null.</param>
        /// <returns>An instance of <see cref="IKnowledgeBaseContainer"/> initialized with the provided data.</returns>
        public IKnowledgeBaseContainer CreateDefaultDataContainer<TRecord, TModel>(IEnumerable<TModel> data) where TModel : class where TRecord : KnowledgeRecord<TModel>, new();

        /// <summary>
        /// Creates a new instance of a knowledge base container using the specified knowledge base, search request, and
        /// filter.
        /// </summary>
        /// <typeparam name="TRecord">The type of the knowledge record contained in the knowledge base.</typeparam>
        /// <typeparam name="TSearchFilter">The type of the search filter used in the knowledge base.</typeparam>
        /// <typeparam name="TSearchSettings">The type of the search settings used in the knowledge base.</typeparam>
        /// <typeparam name="TFilter">The type of the model filter applied to the knowledge base.</typeparam>
        /// <typeparam name="T">The type of the model that the filter applies to.</typeparam>
        /// <param name="knowledgeBase">The knowledge base from which the container is created. Cannot be null.</param>
        /// <param name="knowledgeSearchRequest">The search request containing the filter and settings for the search. Cannot be null.</param>
        /// <param name="filter">The filter to apply to the knowledge base. Can be null if no filtering is required.</param>
        /// <returns>A new instance of <see cref="IKnowledgeBaseContainer"/> configured with the specified parameters.</returns>
        public IKnowledgeBaseContainer CreateContainer<TRecord, TSearchFilter, TSearchSettings, TFilter, T>(IKnowledgeBase<TRecord, TSearchFilter, TSearchSettings, TFilter, T> knowledgeBase, IKnowledgeSearchRequest<TSearchFilter, TSearchSettings> knowledgeSearchRequest, TFilter? filter)
            where TRecord : class, IKnowledgeRecord
            where TSearchFilter : class, IKnowledgeBaseSearchFilter
            where TSearchSettings : class, IKnowledgeBaseSearchSettings
            where TFilter : class, IModelFilter<T>
            where T : class;

        /// <summary>
        /// Creates a default search pipeline using the specified knowledge base containers and optional configurations.
        /// </summary>
        /// <remarks>This method provides a convenient way to create a search pipeline with default
        /// behavior, while allowing optional customization through the provided parameters. The pipeline integrates the
        /// specified knowledge base containers and can be tailored using the optional configurations.</remarks>
        /// <param name="containers">A collection of knowledge base containers that provide the data sources for the pipeline.</param>
        /// <param name="promptConf">An optional configuration for customizing the prompt behavior. If null, a default configuration is used.</param>
        /// <param name="pipelineSettings">Optional additional settings for the pipeline. If null, default settings are applied.</param>
        /// <param name="kernelData">Optional kernel configuration data used to initialize the pipeline. If null, the kernel is initialized with
        /// default values.</param>
        /// <param name="kernel">An optional kernel instance to be used by the pipeline. If null, a new kernel instance is created.</param>
        /// <returns>An <see cref="ErrorOr{T}"/> containing the created <see cref="PipelineModel"/> if successful, or an error if
        /// the pipeline creation fails.</returns>
        public ErrorOr<PipelineModel> CreateDefaultSearchPipeline(IEnumerable<IKnowledgeBaseContainer> containers, PromptConfiguration? promptConf = null, PipelineAdditionalSettings? pipelineSettings = null, KernelConfiguration? kernelData = null, Kernel? kernel = null);


        /// <summary>
        /// Creates a default search pipeline model without a generation step.
        /// </summary>
        /// <remarks>This method is designed to create a search pipeline that excludes any generation
        /// steps, focusing solely on search-related functionality.</remarks>
        /// <param name="containers">A collection of knowledge base containers that provide the context for the search pipeline.</param>
        /// <param name="promptConf">An optional prompt configuration to customize the behavior of the pipeline. If null, a default configuration
        /// is used.</param>
        /// <param name="pipelineSettings">Optional additional settings for the pipeline. If null, default settings are applied.</param>
        /// <param name="kernelData">Optional kernel configuration data to initialize the pipeline. If null, the pipeline uses default kernel
        /// settings.</param>
        /// <param name="kernel">An optional kernel instance to be used by the pipeline. If null, a new kernel instance is created.</param>
        /// <returns>An <see cref="ErrorOr{T}"/> containing the created <see cref="PipelineModel"/> if successful, or an error if
        /// the pipeline creation fails.</returns>
        public ErrorOr<PipelineModel> CreateDefaultSearchPipelineWithoutGenerationStep(IEnumerable<IKnowledgeBaseContainer> containers, PromptConfiguration? promptConf = null, PipelineAdditionalSettings? pipelineSettings = null, KernelConfiguration? kernelData = null, Kernel? kernel = null);

        /// <summary>
        /// Creates the default pipeline steps for performing a Google search using the specified API key and engine.
        /// </summary>
        /// <remarks>This method generates a sequence of pipeline steps that can be used to perform a
        /// Google search.          The pipeline steps are configured based on the provided parameters, allowing for
        /// customization of the search process.</remarks>
        /// <param name="googleApiKey">The API key used to authenticate with the Google Custom Search JSON API.</param>
        /// <param name="googleEngine">The search engine identifier for the Google Custom Search JSON API.</param>
        /// <param name="searchFilter">An optional filter model to refine the search results. If null, no additional filtering is applied.</param>
        /// <param name="googleSettings">Optional settings to configure the behavior of the Google search pipeline. If null, default settings are
        /// used.</param>
        /// <param name="filter">An optional URL record filter to exclude or include specific URLs in the search results. If null, no URL
        /// filtering is applied.</param>
        /// <param name="useScraper">A boolean value indicating whether to use a web scraper for additional data extraction.          <see
        /// langword="true"/> to enable scraping; otherwise, <see langword="false"/>.</param>
        /// <returns>An enumerable collection of pipeline steps that can be used to execute a Google search with the specified
        /// parameters.</returns>
        public IEnumerable<IPipelineStep> CreateDefaultGoogleSearchPipelineSteps(string googleApiKey, string googleEngine, GoogleSearchFilterModel? searchFilter = null, GoogleSettings? googleSettings = null, UrlRecordFilter? filter = null, bool useScraper = false);

        /// <summary>
        /// Downloads the <see cref="EnhancerConfiguration"/> to a file.
        /// </summary>
        /// <param name="configuration">The configuration object to be downloaded. Cannot be <see langword="null"/>.</param>
        /// <param name="filePath">The path of the file where the configuration will be saved. Defaults to "config.json" if not specified.</param>
        /// <param name="hideSecrets">A value indicating whether sensitive information in the configuration should be masked.  <see
        /// langword="true"/> to hide secrets; otherwise, <see langword="false"/>. Defaults to <see langword="true"/>.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public Task DownloadConfiguration(EnhancerConfiguration configuration, string filePath = "config.json", bool hideSecrets = true);

        /// <summary>
        /// Exports the <see cref="EnhancerConfiguration"/> to a byte array.
        /// </summary>
        /// <param name="configuration">The configuration to export. Cannot be <see langword="null"/>.</param>
        /// <param name="hideSecrets">A value indicating whether sensitive information, such as secrets, should be excluded from the exported
        /// data. <see langword="true"/> to hide secrets; otherwise, <see langword="false"/>.</param>
        /// <returns>A byte array representing the exported configuration.</returns>
        public byte[] ExportConfigurationToBytes(EnhancerConfiguration configuration, bool hideSecrets = true);

        /// <summary>
        /// Imports an <see cref="EnhancerConfiguration"/> from the specified file.
        /// </summary>
        /// <param name="filePath">The path to the file containing the configuration to import. The file must exist and be in a valid format.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the imported <see
        /// cref="EnhancerConfiguration"/> if the operation is successful; otherwise, <see langword="null"/> if the file
        /// is empty or the configuration could not be imported.</returns>
        public Task<EnhancerConfiguration?> ImportConfigurationFromFile(string filePath);

        /// <summary>
        /// Imports an <see cref="EnhancerConfiguration"/> object from a JSON-encoded byte array.
        /// </summary>
        /// <param name="jsonBytes">A byte array containing the JSON representation of the configuration.</param>
        /// <returns>An <see cref="EnhancerConfiguration"/> object if the import is successful; otherwise, <see
        /// langword="null"/>.</returns>
        public EnhancerConfiguration? ImportConfigurationFromBytes(byte[] jsonBytes);
    }
}
