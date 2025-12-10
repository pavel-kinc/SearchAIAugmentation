# PromptEnhancer - For Prompt Augmentation
The main goal of the project is a library that provides a flexible, highly configurable **RAG-based pipeline** that systematically uses Knowledge Bases to find relevant context, augment the user's base prompt with that information, and prepare the augmented prompt for reliable output generation by a Large Language Model (LLM). It also provides utilities for utilizing LLMs and other AI services and using the **Semantic Kernel**.

It includes **two demonstration applications** built on the library to showcase various use cases.

This repository contains several projects: 
| Project Name | Role & Description |
| :--- | :--- |
| **[`PromptEnhancer`](#the-library---promptenhancer)** | The **library PromptEnhancer** containing all core logic, generic interfaces, kernel management, and extension methods for prompt augmentation. |
| **[`ConfigurableGoogleSearchDemo`](#the-google-demo)** | Configurable **Google Demo** showcasing usage of the library by leveraging Google Search Engine for relevant context and subsequent augmentation. |
| **[`TaskChatDemo`](#the-task-chat-demo)** | **Chat demo** focusing on using multiple AI providers, getting context from multiple Knowledge Bases working with development tasks. Uses other project **TaskDemoAPI** |
| **`AppHost`** | The **.NET Aspire Orchestrator** responsible for defining, configuring, and launching all application projects in the solution.|
**`ServiceDefaults`** | The **Shared Configuration Project** that centralizes common service configurations, such as logging, tracing, and health checks, used by all demo apps.
| **`PromptEnhancer.Tests`** | The **Test** project, ensuring the core library's functionality and interactions with services are robust and correct. |

## Technology

This entire solution is built using the **.NET 9** platform, leveraging the latest advancements in performance and stability from the .NET ecosystem. 

## Cooperation

The development of the solution is the subject of a **Master Thesis**, conducted in **close cooperation with company BiQ pux a.s.**

## Safe Usage Disclaimer

Please be aware of the functionality within the solution (as it is highly configurable):
* It remains the user's sole responsibility to ensure all final outputs and operational practices are used responsibly, ethically, and strictly within legal boundaries.
* The **`ConfigurableGoogleSearchDemo`** project is designed to integrate external web search or scraping tools to augment prompts with real-time data.
* **By default, this external data feature is deactivated and must be explicitly configured and enabled by the user.**

**Legal Compliance and Terms of Service:** Users are solely responsible for ensuring that any enabled external integration complies fully with the **Terms of Service, Acceptable Use Policies (AUPs), and all applicable laws and regulations** of the respective external service providers (e.g., Google, LLM APIs, etc.). We strongly advise users to exercise caution, respect service terms and conditions, and adhere to rate limits when activating any external search or scraping functionality. Use of such tools is done at your own risk.

## The Library - PromptEnhancer
The PromptEnhancer library provides a flexible, configurable **Retrieval-Augmented Generation based pipeline system** that automatically searches for relevant context and uses it to systematically augment a user's base prompt. It contains steps from processing user input to final LLM generation for various tasks. This process prepares a highly structured, context-rich query for reliable output generation by a Large Language Model (LLM). The library uses **Semantic Kernel** for working with AI services. It is provider agnostic, meaning it can interact with any supported AI service (e.g., OpenAI, Gemini, Claude) - AI service is for exmample embedding generation and LLM text generation (inference).

Learn more about the library and surrounding solution with demonstration apps on github  <a href="https://github.com/pavel-kinc/SearchAIAugmentation" target="_blank">here</a>.
### Overview
#### Safe Usage Disclaimer
Please be aware of the functionality within the solution (as it is highly configurable):
* It remains the user's sole responsibility to ensure all final outputs and operational practices are used responsibly, ethically, and strictly within legal boundaries.
* If you decide to use the Google search and scraper feature (a lightweight scraper that does not attempt to bypass rate limits or complex security measures), you are solely responsible for ensuring its use is in legal compliance.
#### Installation
To incorporate the core library into your .NET project, install the NuGet package:
```bash
dotnet add package PromptEnhancer
```
Or you can add it through NuGet Package Manager in IDE.
#### Additional information
The PromptEnhancer library is a flexible, configurable .NET 9 RAG-based pipeline system currently in Beta status, functionality, interfaces and API may change in the near future with new releases.

For demonstration purposes, I created 2 Demo applications, that showcase, how the library can be used for real tasks:

* <a href="https://github.com/pavel-kinc/SearchAIAugmentation?tab=readme-ov-file#the-google-demo" target="_blank">GoogleSearchDemo</a>
* <a href="https://github.com/pavel-kinc/SearchAIAugmentation?tab=readme-ov-file#the-task-chat-demo" target="_blank">TaskChatDemo</a>

### Main functionality
To use the library, you must register the required services in your application's service container. You can configure it with parameters (they are all implicitly set).
The main 2 interfaces to leverage the functionality of the library are **IEnhancerService** and **ISemanticKernelManager**.

There are multiple ways to get to generation, pipeline execution, steps, configurations, and other setups to make the library flexible enough for distinct applications. One of the most important thing is to setup Knowledge Base Containers with Knowledge Bases.
```csharp
// Program.cs (or Startup.cs)
builder.Services.AddPromptEnhancer();
```

Now we can inject these interfaces into a Consumer/Component:


```csharp
public class LibraryUtilityService
{
    private readonly IEnhancerService _enhancerService;
    private readonly ISemanticKernelManager _semanticKernelManager;

    public LibraryUtilityService(IEnhancerService enhancerService, ISemanticKernelManager skManager)
    {
        _enhancerService = enhancerService;
        _semanticKernelManager = skManager;
    }
}
```
#### Configuration
Before we start working with pipeline, we need to setup EnhancerConfiguration, easiest way to do this is to call method *CreateDefaultConfiguration()*, parameters are configured to create basic configuration, with implicit AIProvider set to OpenAI with models. If you choose to create Kernel instance this way, you need to provide ApiKey. (Other ways would be to send your own Kernel instance, or setup Kernel in DI)
```csharp
// creates config for OpenAI provider, with Model = "gpt-4o-mini" and EmbeddingModel = "text-embedding-3-small", it is configurable with parameters
var enhancerConfig = _enhancerService.CreateDefaultConfiguration(_configuration["AIServices:OpenAI:ApiKey"]);

// now we can configure our settings with desired values or user input, there is a lot to setup, below is just Additional instructions for SystemPrompt
enhancerConfig.PromptConfiguration.AdditionalInstructions =
    """
    You are an AI assistant helping the user to find the location of toys.
    """;
```
#### Pipeline
Now, with configuration created, we can use it to setup and execute a pipeline, again there are multiple ways to setup pipeline and configurations. This shows the most simplest way to setup Knowledge Base Container with some Data, for higher degree of control and functionality, you need to create your own KnowledgeBase (this will be shown later, or can be found in the demo applications).
```csharp
// data
var data = new List<string>
{
    "The red ball is in the living room.",
    "The blue car is in the garage.",
    "The teddy bear is on the bed in the bedroom.",
    "The puzzle pieces are in the playroom."
};

// simplest way to create a Knowledge Base container, it is recommended you use your KnowledgeBase to search for relevant context, this creates Knowledge Base in the background
var dataContainer = _enhancerService.CreateDefaultDataContainer(data);

// we get these steps with implicit arguments:
// PreprocessStep, MultipleSearchStep R,
// ProcessEmbeddingStep R, ProcessRankStep R,
// ProcessFilterStep R, PostProcessCheckStep,
// PromptBuilderStep R, and GenerationStep R
// where R means required
var pipelineSteps = _enhancerService.CreateDefaultSearchPipelineSteps([dataContainer]);

// specify user entry
var entry = new Entry() { QueryString = "Where is the red ball?"};

// now we can call the pipeline - both embedding and generation services are called in this case!
var result = await _enhancerService.ProcessConfiguration(enhancerConfig, entry);

if (result.IsError)
{
    // error
    Console.WriteLine("System: There was an error");
}
else
{
    // here we can expect something like "The red ball is located in the living room." or null
    Console.WriteLine($"System: {result.Value.Result?.FinalResponse}");
}
```

You can also leverage other methods like *ExecutePipelineAsync* that takes 'PipelineModel' and 'IEnumerable\<PipelineRun> entries' - where you can setup your own settings and run partial or full pipelines based on steps or the given PipelineRun. There is also method for creating pipeline settings *CreatePipelineSettingsFromConfig*. For more insights on how to use these and other methods, please refer to the [`demonstration applications`](#additional-information).

### Full demonstration
Now to full demonstration using Knowledge Base (in this case for Google with full step definitions) from DI:
```csharp
var enhancerConfiguration = _enhancerService.CreateDefaultConfiguration(_configuration["AIServices:OpenAI:ApiKey"]);

// creating request model
var request = new GoogleSearchRequest
{
    // basic settings for google search
    Settings = new GoogleSettings
    {
        SearchApiKey = _configuration["SearchConfigurations:Google:ApiKey"]!,
        Engine = _configuration["SearchConfigurations:Google:SearchEngineId"]!,
        // it is by default false, it uses just the search result snippets
        UseScraper = false,
    },
    // you can setup google search parameter filters if needed
    Filter = new GoogleSearchFilterModel(),
};

// we create a container that uses google knowledge base to search for relevant data, filter of null means no filter
// google knowledge base is injected in constructor - GoogleKnowledgeBase googleKnowledgeBase
// it is part of the library, you do not have to create it
var container = _enhancerService.CreateContainer(_googleKnowledgeBase, request, null);
enhancerConfiguration.Steps = new List<IPipelineStep>
{
    new PreprocessStep(),
    new MultipleSearchStep([container], allowAutoChoice: true, isRequired: true),
    new ProcessEmbeddingStep(skipGenerationForEmbData: true, isRequired: true),
    new ProcessRankStep(isRequired: true),
    new ProcessFilterStep(new RecordPickerOptions(){MinScoreSimilarity = 0.2d, Take = 5, OrderByScoreDescending = true}, isRequired: true),
    new PostProcessCheckStep(),
    new PromptBuilderStep(isRequired: true),
    new GenerationStep(isRequired: true)
};

// specify user entry
var entry = new Entry() { QueryString = "What is Semantic Kernel?" };

// now we can call the pipeline
var result = await _enhancerService.ProcessConfiguration(enhancerConfiguration, entry);

if (result.IsError)
{
    Console.WriteLine("System: There was an error");
}
else
{
    // here we can expect description of what Semantic Kernel
    Console.WriteLine($"System: {result.Value.Result?.FinalResponse}");
}
```
Here we could have skipped creating the steps and Google request with utility method from *IEnhancerService*:
```csharp
// once again it has multiple configurable arguments
enhancerConfiguration.Steps = _enhancerService.CreateDefaultGoogleSearchPipelineSteps(googleApiKey, engine, useScraper: false);
```
It is designed this way for users to have maximum control over searching in relevant knowledge bases.

The Google Knowledge Base could look like this (the real is a bit different):
```csharp
// example of Google Knowledge Base - you need to implement SearchAsync
public class GoogleKnowledgeBase : KnowledgeBaseUrl<KnowledgeUrlRecord, GoogleSearchFilterModel, GoogleSettings, UrlRecordFilter, UrlRecord>
{
    private readonly ISearchProviderManager _searchProviderManager;

    public GoogleKnowledgeBase(ISearchWebScraper searchWebScraper, IChunkGeneratorService chunkGenerator, ISearchProviderManager searchProviderManager)
        : base(searchWebScraper, chunkGenerator)
    {
        _searchProviderManager = searchProviderManager;
    }

    public override string Description => $"This knowledge base uses Google Search to retrieve up-to-date data. Can be used for various queries that could use data from web search.";

    public async override Task<IEnumerable<KnowledgeUrlRecord>> SearchAsync(
        IKnowledgeSearchRequest<GoogleSearchFilterModel, GoogleSettings> request, IEnumerable<string> queriesToSearch, UrlRecordFilter? filter = null, CancellationToken ct = default)
    {
        // here is the setup for Google Search, fetching data, filtering and KnowledgeRecord creation
    }
}
```
### Working with Kernel
Kernel primarily provides basic AI services. The pipeline requires a Kernel and there are multiple ways to configure it, you create it on your own (for example with help from *ISemanticKernelManager*)and pass it, through DI, or through *KernelConfiguration*. Until now we created it with help of default *KernelConfiguration*.

Kernel itself is in the *PipelineModel* in *PipelineSettings*.

1. **Creating Kernel**
You can supply your own Kernel to Pipeline processing and execution. Check for null values.
```csharp
#pragma warning disable SKEXP0010 
// Option 1: create Kernel manually
var kernel = Kernel.CreateBuilder()
    .AddOpenAIChatClient("model", "apiKey")
    .AddOpenAIEmbeddingGenerator("embeddingModel", "apiKey")
    .Build();
#pragma warning restore SKEXP0010

// Option 2: create Kernel using manager, that is configurable and adds functionality like context plugins
// you can also leverage creating it with KernelConfiguration - this is used with Configuration creation
var kernel2 = _kernelManager.CreateKernel(
    [
    new (AIProviderEnum.OpenAI, "gpt-4o-mini", "apiKey"),
    new (AIProviderEnum.OpenAI, "text-embedding-3-small", "apiKey", serviceType: KernelServiceEnum.EmbeddingGenerator),
    ]
).Value;

// then you can use it like this
await _enhancerService.ProcessConfiguration(enhancerConfiguration, entry, kernel: kernel!);
```

2. **Kernel from DI** You can add it to DI container. Then it is resolved on its own in the *IEnhancerService*.
```csharp
// Option 1: You add it to DI yourself
builder.Services.AddKernel()
    .AddOpenAIChatClient("model", "apiKey")
    .AddOpenAIEmbeddingGenerator("embeddingModel", "apiKey");

// Option 2: Use the PromptEnhancer - you can either supply the AI services before (KernelBuilder will resolve them), or supply it to the method
builder.Services.AddPromptEnhancer(addKernelToDI: true);
```

3. ***KernelConfiguration*** You can use configuration to define kernel, you can also use this to create your own Kernel instance with the help of *ISemanticKernelManager*.
```csharp
// this will create both AI services with different models
var kernelConfig = new KernelConfiguration() { AIApiKey = "apiKey", Model = "model", EmbeddingModel = "embModel", UseLLMConfigForEmbeddings = true};

// then you can use it
enhancerConfiguration.KernelConfiguration = kernelConfig;
await _enhancerService.ProcessConfiguration(enhancerConfiguration, entry);
```
The priority of Kernel is from 1. to 3.

### Core Components
The library's design philosophy ensures that most components are public, preventing it from operating as a black box and offering users full transparency and control over the RAG pipeline's inner workings.
#### Knowledge base - The Core Retrieval Component
The IKnowledgeBase interface is the central retrieval abstraction for the PromptEnhancer RAG-based pipeline, though it is not a traditional data container itself. Instead, it serves as a generic component that encapsulates all the necessary logic for context searching, primarily through the key method SearchAsync. This interface defines the complete retrieval workflow, including connecting to the data source, performing chunking (dividing data into segments), creating a specific KnowledgeRecord from the original data model, and applying necessary filters or search parameters defined by generics. Logic that is common across implementations (like basic chunking) is housed in the abstract KnowledgeBase class, and each concrete implementation must explicitly define the context it searches for, which is essential for automatic selection within the pipeline.

The design emphasizes flexibility, allowing the KnowledgeRecord creation process to be overridden to define specific LLM or embedding representations and to support external embedding sources. While this powerful system allows users to define exactly where and how data is retrieved, its current architecture reflects the continuous design evolution of the project, leading to some internal complexity. It is recommended to create concrete implementations of IKnowledgeBase using a Dependency Injection (DI) container for easier management of dependencies; future development aims to modularize this component further for a lighter, more streamlined use.

Example from Task Chat Demo:
```csharp
public class TaskDataKnowledgeBase : KnowledgeBase<KnowledgeRecord<TaskItemData>, SearchItemFilterModel, ItemDataSearchSettings, EmptyModelFilter<TaskItemData>, TaskItemData>
{
    // internal source
    public IVectorStoreService _vectorStoreService;
    public TaskDataKnowledgeBase(IVectorStoreService vectorStoreService)
    {
        _vectorStoreService = vectorStoreService;
    }
    public override string Description => 
        """
        This knowledge base uses TaskData, that contain information about development tasks (who did what). 
        Any standalone (out of place) part of the query is probably referencing this base.
        """;
    public async override Task<IEnumerable<KnowledgeRecord<TaskItemData>>> SearchAsync(
        IKnowledgeSearchRequest<SearchItemFilterModel, ItemDataSearchSettings> request, 
        IEnumerable<string> queriesToSearch, EmptyModelFilter<TaskItemData>? filter = null, 
        CancellationToken ct = default)
    {
        // work only with the first user query
        var query = queriesToSearch.FirstOrDefault();
        if (string.IsNullOrWhiteSpace(query))
        {
            // return empty list
            return [];
        }
        // create embedding for user query
        var queryVector = await request.Settings.Generator.GenerateVectorAsync(query, cancellationToken: ct);
        // get relevant data using search and semantic search
        var data = await _vectorStoreService.GetDataWithScoresAsync(queryVector, request.Filter);
        // convert data to KnowledgeRecords
        var res = GetKnowledgeRecords(data, filter, query, false, ct: ct);
        return res;
    }

    // override for creating KnowledgeRecord because of given SimilarityScore of the given Vector Store
    // Disclaimer! it is not good to combine similarity scores calculated by different models
    protected override KnowledgeRecord<TaskItemData> CreateRecord(TaskItemData o, string queryString)
    {
        return new KnowledgeRecord<TaskItemData>
        {
            Id = Guid.NewGuid().ToString(),
            SourceObject = o,
            Source = GetType().Name,
            UsedSearchQuery = queryString,
            //adding similarity score here
            SimilarityScore = o.SimilarityScore,
        };
    }
}
```
#### Knowledge record
The *KnowledgeRecord<T>* class is the core component for abstracting a single piece of knowledge (context) within the RAG-based Pipeline, implementing the *IKnowledgeRecord* interface. It is designed to wrap the original retrieved data and contain all the meta-information needed for subsequent processing, ensuring the pipeline has full context about the data it's injecting into the prompt.

```csharp
// the default Knowledge Record class
public class KnowledgeRecord<T> : IKnowledgeRecord
    where T : class
{
    // Default JsonSerializerOptions with specific Unicode ranges
    public static readonly JsonSerializerOptions Default = new JsonSerializerOptions
    {
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.LatinExtendedA, UnicodeRanges.Latin1Supplement),
    };

    public string? Id { get; set; }
    public T SourceObject { get; set; }
    public IDictionary<string, string>? Metadata { get; set; }

    /// <summary>
    /// Source of the given Knowledge Record.
    /// </summary>
    public string Source { get; set; }
    // optional precomputed embeddings, use only with same model!
    public PipelineEmbeddingsModel? Embeddings { get; set; }

    public double? SimilarityScore { get; set; }

    public string UsedSearchQuery { get; set; }

    /// <summary>
    /// Gets an expression that selects a chunk identifier for an entity of type <typeparamref name="T"/>.
    /// </summary>
    public virtual Expression<Func<T, string?>>? ChunkSelector => null;

    /// <summary>
    /// Gets the string representation of the object suitable for use in large language model (LLM) interactions.
    /// </summary>
    public string LLMRepresentationString { get; } => JsonSerializer.Serialize(SourceObject, Default);

    /// <summary>
    /// Gets the string representation of the object used for Embedding generation.
    /// </summary>
    public string EmbeddingRepresentationString { get; } => JsonSerializer.Serialize(SourceObject, Default);

    object IKnowledgeRecord.SourceObject => SourceObject;
}
```
You can create your own class, that overrides the defaults:
```csharp
    public class KnowledgeUrlRecord : KnowledgeRecord<UrlRecord>
    {
        public override Expression<Func<UrlRecord, string?>>? ChunkSelector => x => x.Content;
        public override string EmbeddingRepresentationString => SourceObject!.Content;
    }
```
#### Pipeline system
The Pipeline represents the core of the execution engine for the entire library. Its primary function is to ensure the sequential execution of a defined series of steps, formalized by the IPipelineStep interface, which together form a complete RAG-based data flow. The pipeline is designed for high flexibility; individual steps are highly configurable and often contain default settings, ensuring ease of use for general scenarios.

Semantic Kernel is utilized within the pipeline solely for providing basic AI services, such as calling the LLM and generating Embeddings. Its advanced orchestration capabilities are leveraged in the KernelContextPluginsStep, where it orchestrates obtaining context for the user input. This context retrieval is enabled by registering plugins that implement the ISemanticKernelContextPlugin interface within the Kernel.

To work with pipeline utilize these two methods from IEnhancerService Interface: 

**ProcessConfiguration** - Processes *EnhancerConfiguration*, creates *PipelineModel* with Kernel and settings, then parallelly runs the entries (from *Entry* is created default *PipelineRun*).
**ExecutePipelineAsync** - *PipelineModel* and *PipelineRun* given, runs the pipeline with the given entries. Good for partial pipelines, or reusing the results.

Both methods return *Task<ErrorOr\<IList\<PipelineResultModel>>>*, *PipelineResultModel* contains information about each entry Run - if it finished as a success or a failure. Most importantly it contains PipelineRun that has all the data gotten from the pipeline execution.

```csharp
// the main result of the pipeline Execution
public class PipelineRun
{
    public PipelineRun(Entry? entry = null)
    {
        Entry = entry;
        QueryString = entry?.QueryString;
    }
    public string? QueryString { get; set; }

    public IEnumerable<string> QueryStrings { get; set; } = [];

    public List<IKnowledgeRecord> RetrievedRecords { get; init; } = [];

    public IEnumerable<IKnowledgeRecord> PickedRecords { get; set; } = [];

    public List<string> AdditionalContext { get; init; } = [];

    public string? SystemPromptToLLM { get; set; }

    public string? UserPromptToLLM { get; set; }
    public IEnumerable<ChatMessage>? ChatHistory { get; set; }
    public List<string> PipelineLog { get; set; } = [];

    public IDictionary<string, object> Metadata { get; init; } = new Dictionary<string, object>();

    public ChatResponse? FinalResponse { get; set; }

    public Entry? Entry { get; init; }

    public long InputTokenUsage { get; set; } = 0;

    public long OutputTokenUsage { get; set; } = 0;

    public long TokenUsage => InputTokenUsage + OutputTokenUsage;
}
```
#### Configurations and settings
Integration with modern AI systems necessitates managing a large number of settings that fundamentally impact the quality, cost, and flow of communication with the AI. Consequently, the PromptEnhancer library's architecture is designed to be fully configuration-driven, requiring not only external AI service settings but also its own internal configuration classes to specify behavior and execution logic.

The central configuration class for working with the library is *EnhancerConfiguration*. This class serves as a comprehensive configuration node, collecting all necessary sub-configurations and settings required for the library's operation:
* ***KernelConfiguration***: An optional configuration used primarily for setting up the underlying Semantic Kernel instances required for basic AI services (e.g., connecting to LLM providers for inference and embedding generation).
* ***PromptConfiguration***: Contains settings that control the generated prompts, such as defining system instructions for the LLM, specifying language, and identifying the default model instance to be used if not specified elsewhere.
* ***PipelineAdditionalSettings***: Holds supplementary settings that affect the overall behavior of the Pipeline. This includes optional interaction settings for the LLM:
*ChatOptions* and *PromptExecutionSettings*. ChatOptions contains general settings related to communication with the LLM. *PromptExecutionSettings* used to define all parameters and specific behaviors that affect how the LLM model executes the final prompt (e.g., temperature, max tokens, stop sequences).
* **Steps (*IEnumerable\<IPipelineStep>*)**: A collection of the individual IPipelineStep components that explicitly define the entire sequential execution flow and logic of the RAG-based Pipeline.
* ***PipelineSettings***: Created settings for given pipeline, that are present in *PipelineModel* along with steps. These settings contain before mentioned *PipelineAdditionalSettings*, Kernel, ServiceProvider and *PromptConfiguration*
* **More settings, configurations and filters** can defined by user for certain operations in pipeline or for Knowledge Bases

### License
The PromptEnhancer library is released using the LGPL-2.1 license.

## The Google Demo
This is the first demo, it leverages the Google Search Engine to retrieve relevant data. It is highly configurable in UI (even though not everything, the library itself has more options). It is configured to generate SEO descriptions for products. It needs a secret file **'*appsettings.secrets.json*'** where are the AI api keys and Google search api and engineId.

This demo uses dynamic context with the help of Google Knowledge Base and augments data either with snippets or with scraped content (if the scraping is allowed). It includes a lightweight scraper that does not try to bypass security in any way, and on anything but a 200 HTTP response does not scrape. Either way, it is up to the user to use this feature responsibly and in legal compliance (and possible copyright issues). It is recommended that users use this feature with a configuration, that can fetch only publicly accessible data, or to use the retrieved and generated content responsibly and in legal boundaries.

For the Google Chat Demo, the decision was made to keep the Google Search service implementation and configuration directly within the library. This allows users to immediately leverage its functionality in their RAG-based pipelines.

* **Dynamic Knowledge Base**: By utilizing Google Search, the library provides access to a dynamic knowledge base whose vast breadth and timeliness of data are highly effective for RAG-based use cases that require up-to-date information from the public internet.

* **Reference Implementation**: This integrated structure also serves as a reference implementation for users who wish to build and configure their own scenarios involving dynamic, real-time context retrieval.

## The Task Chat Demo
The TaskDemoChat is a contrasting demonstration application designed to showcase a complex, multi-source Retrieval-Augmented Generation configuration, emphasizing the management of chat history (conversational context), the automatic selection of multiple Knowledge Bases and streaming of the final LLM text generation.

The demo simulates an internal assistant for working with information related to development tasks (e.g., Jira Tasks, Azure DevOps Work Items). Its primary goal is to handle two scenarios: searching for specific task records and engaging in general conversation while maintaining a continuous conversational thread for a smoother user experience. It uses data generated by ChatGPT for the task records.

The demo employs a hybrid approach using two different AI providers, Gemini for text generation and OpenAI for embedding creation. It also requires the ***appsettings.secrets.json*** in root project.

The application demonstrates the library's ability to automatically combine and select context from three distinct, hybrid Knowledge Base sources simultaneously:
* **External API Application**: A dedicated API application provides structured Task records (WorkItem data) and must be accessible to the demo.

* **In-Memory Vector Store**: A private, in-memory store contains pre-generated embedding representations for internal TaskData. This store uses the text-embedding-3-small model for searching and assigns a similarity score internally.

* **Google Search**: Used in a lightweight capacity; it skips the scraper and chunking steps, taking only the resulting search snippets from the user input as context.

## The Secrets File template
The demo applications need a secret json file in their project root to work correctly. It is loaded through IConfiguration in *Program.cs*.
```json
// *appsettings.secrets.json*
{
  "AIServices": {
    "OpenAI": {
      "ApiKey": "apiKey"
    },
    "Gemini": {
      "ApiKey": "apiKey"
    }
  },
  "SearchConfigurations": {
    "Google": {
      "ApiKey": "apiKey",
      "SearchEngineId": "engineId"
    }
  }
}
```