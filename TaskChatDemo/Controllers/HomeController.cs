using ErrorOr;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel;
using PromptEnhancer.KnowledgeBaseCore;
using PromptEnhancer.KnowledgeBaseCore.Examples;
using PromptEnhancer.KnowledgeBaseCore.Interfaces;
using PromptEnhancer.KnowledgeRecord;
using PromptEnhancer.KnowledgeRecord.Interfaces;
using PromptEnhancer.KnowledgeSearchRequest;
using PromptEnhancer.KnowledgeSearchRequest.Examples;
using PromptEnhancer.Models;
using PromptEnhancer.Models.Configurations;
using PromptEnhancer.Models.Enums;
using PromptEnhancer.Models.Examples;
using PromptEnhancer.Models.Pipeline;
using PromptEnhancer.Pipeline;
using PromptEnhancer.Pipeline.PromptEnhancerSteps;
using PromptEnhancer.Services.EnhancerService;
using PromptEnhancer.Services.PromptBuildingService;
using PromptEnhancer.SK.Interfaces;
using System.Diagnostics;
using System.Text.Json;
using TaskChatDemo.KnowledgeBases;
using TaskChatDemo.Models;
using TaskChatDemo.Models.SearchFilterModels;
using TaskChatDemo.Models.Settings;
using TaskChatDemo.Models.TaskItem;
using TaskChatDemo.Services.ApiConsumer;

namespace TaskChatDemo.Controllers;

public class HomeController : Controller
{
    private const string ChatHistory = "EntryKey";
    private readonly IPromptBuildingService promptBuildingService;
    private readonly ILogger<HomeController> _logger;
    private readonly IEnhancerService _enhancerService;
    private readonly IConfiguration _configuration;
    private readonly ISemanticKernelManager _semanticKernelManager;
    private readonly IServiceProvider _serviceProvider;
    private readonly VectorStoreCollection<Guid, TaskItemModel> _taskCollection;
    private readonly IWorkItemApiService _workItemApiService;
    private readonly TaskDataKnowledgeBase _taskDataKnowledgeBase;
    private readonly WorkItemKnowledgeBase _workItemKnowledgeBase;
    private readonly GoogleKnowledgeBase _googleKB;

    public HomeController(IEnhancerService enhancerService, ILogger<HomeController> logger, IConfiguration configuration, ISemanticKernelManager skManager, IServiceProvider serviceProvider, VectorStoreCollection<Guid, TaskItemModel> taskCollection, IWorkItemApiService workItemApiService, TaskDataKnowledgeBase taskDataKnowledgeBase, WorkItemKnowledgeBase workItemKnowledgeBase, GoogleKnowledgeBase googleKB, IPromptBuildingService promptBuildingService)
    {
        _enhancerService = enhancerService;
        _logger = logger;
        _configuration = configuration;
        _semanticKernelManager = skManager;
        _serviceProvider = serviceProvider;
        _taskCollection = taskCollection;
        _workItemApiService = workItemApiService;
        _taskDataKnowledgeBase = taskDataKnowledgeBase;
        _workItemKnowledgeBase = workItemKnowledgeBase;
        _googleKB = googleKB;
        this.promptBuildingService = promptBuildingService;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [HttpGet("/chat/stream")]
    public async Task Chat(string q, bool skipPipeline = false, CancellationToken ct = default)
    {
        var chatHistory = HttpContext.Session.GetString(ChatHistory) is string json
                ? JsonSerializer.Deserialize<List<ChatMessage>>(json)
                : null;
        HttpContext.Session.SetString(ChatHistory, JsonSerializer.Serialize(chatHistory));
        var entry = new Entry() { QueryString = q };

        var settingsResult = GetPipelineSettings();
        if (settingsResult.IsError)
        {
            //return RedirectToAction("Error");
        }
        var settings = settingsResult.Value;

        var context = await GetNewContext(q, skipPipeline, entry, settings);
        context.ChatHistory ??= chatHistory;

        chatHistory = await HandleStreamingMessage(chatHistory, settings, context, ct);
        HttpContext.Session.SetString(ChatHistory, JsonSerializer.Serialize(chatHistory));
        return;
    }

    private async Task<PipelineContext> GetNewContext(string q, bool skipPipeline, Entry entry, PipelineSettings settings)
    {
        PipelineContext context;
        if (skipPipeline)
        {
            context = new PipelineContext(entry)
            {
                UserPromptToLLM = q,
                SystemPromptToLLM = promptBuildingService.BuildSystemPrompt(settings.PromptConfiguration),
            };
        }
        else
        {
            var pipeline = GetPipeline(settings, settings.Kernel);
            var pipelineRes = await _enhancerService.ProcessPipelineAsync(pipeline, [new PipelineContext(entry)]);
            if (pipelineRes.IsError || pipelineRes.Value.FirstOrDefault()?.Result is null)
            {
                //return RedirectToAction("Error");
            }
            context = pipelineRes.Value.FirstOrDefault()!.Result!;
        }

        return context;
    }

    private async Task<List<ChatMessage>> HandleStreamingMessage(List<ChatMessage>? chatHistory, PipelineSettings settings, PipelineContext context, CancellationToken ct)
    {
        var res = _enhancerService.GetStreamingResponse(settings, context);
        List<ChatResponseUpdate> updates = [];

        Response.ContentType = "text/event-stream";
        await Response.StartAsync(ct);
        await foreach (ChatResponseUpdate update in res)
        {
            await Response.WriteAsync($"data: {update}\n\n", ct);
            await Response.Body.FlushAsync(ct);
            updates.Add(update);
        }

        await Response.WriteAsync("event: done\ndata: end\n\n", ct);
        await Response.Body.FlushAsync(ct);
        chatHistory = context.ChatHistory?.ToList() ?? [];
        var chatResponse = updates.ToChatResponse();
        var inputTokens = chatResponse.Usage?.InputTokenCount;
        chatHistory.AddRange(chatResponse.Messages);
        return chatHistory;
    }

    private PipelineModel GetPipeline(PipelineSettings settings, Kernel kernel)
    {
        var taskItemRequest = new KnowledgeSearchRequest<SearchItemFilterModel, ItemDataSearchSettings>()
        {
            Filter = new SearchItemFilterModel(),
            Settings = new ItemDataSearchSettings() { Generator = kernel.GetRequiredService<IEmbeddingGenerator<string, Embedding<float>>>("openai") }
        };
        var workItemRequest = new KnowledgeSearchRequest<SearchWorkItemFilterModel, WorkItemSearchSettings>()
        {
            Filter = new SearchWorkItemFilterModel(),
            Settings = new WorkItemSearchSettings(),
        };

        var containers = new List<IKnowledgeBaseContainer>()
        {
            new KnowledgeBaseContainer<KnowledgeRecord<TaskItemData>, SearchItemFilterModel, ItemDataSearchSettings, EmptyModelFilter<TaskItemData>, TaskItemData>(_taskDataKnowledgeBase, taskItemRequest, null),
            new KnowledgeBaseContainer<KnowledgeRecord<WorkItem>, SearchWorkItemFilterModel, WorkItemSearchSettings, EmptyModelFilter<WorkItem>, WorkItem>(_workItemKnowledgeBase, workItemRequest, null),
        };
        var googleApiKey = _configuration["SearchConfigurations:Google:ApiKey"];
        var googleEngine = _configuration["SearchConfigurations:Google:SearchEngineId"];
        if (!string.IsNullOrEmpty(googleApiKey) && !string.IsNullOrEmpty(googleEngine))
        {
            var request = new GoogleSearchRequest
            {
                Settings = new GoogleSettings
                {
                    SearchApiKey = googleApiKey,
                    Engine = googleEngine,
                    UseScraper = false,
                    AllowChunking = false,
                },
                Filter = new GoogleSearchFilterModel(),
            };
            containers.Add(new KnowledgeBaseContainer<KnowledgeUrlRecord, GoogleSearchFilterModel, GoogleSettings, UrlRecordFilter, UrlRecord>(_googleKB, request, null));
        }

        var steps = new List<PipelineStep>()
        {
            new PreprocessStep(),
            new KernelContextPluginsStep(),
            new MultipleSearchStep(containers, isRequired: true),
            new ProcessEmbeddingStep(skipGenerationForEmbData: true, isRequired: true),
            new ProcessRankStep(isRequired: true),
            new ProcessFilterStep(new RecordPickerOptions(){MinScoreSimilarity = 0.4d, Take = 10, OrderByScoreDescending = true}, isRequired: true),
            new PostProcessCheckStep(),
            new PromptBuilderStep(isRequired: true)
        };
        var pipeline = new PipelineModel(settings, steps);
        return pipeline;
    }

    private ErrorOr<PipelineSettings> GetPipelineSettings()
    {
        var kernel = CreateKernel(_configuration["AIServices:OpenAI:ApiKey"] ?? string.Empty, _configuration["AIServices:Gemini:ApiKey"] ?? string.Empty);
        var enhancerConfig = _enhancerService.CreateDefaultConfiguration(aiApiKey: _configuration["AIServices:OpenAI:ApiKey"]);
        if (kernel is not null)
        {
            enhancerConfig.PipelineAdditionalSettings = new PipelineAdditionalSettings()
            {
                ChatClientKey = "openai",
                GeneratorKey = "gemini",
                KernelRequestSettings = new PromptExecutionSettings()
                {
                    ServiceId = "openai",
                    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
                },
            };
        }

        var settingsResult = _enhancerService.CreatePipelineSettingsFromConfig(enhancerConfig.PromptConfiguration, enhancerConfig.PipelineAdditionalSettings, enhancerConfig.KernelConfiguration, kernel);
        return settingsResult;
    }

    private Kernel? CreateKernel(string openAiApiKey, string geminiApiKey)
    {
        if (string.IsNullOrEmpty(openAiApiKey) || string.IsNullOrEmpty(geminiApiKey))
        {
            return null;
        }
        var configs = new List<KernelServiceBaseConfig>
        {
            new (AIProviderEnum.OpenAI, "gpt-4o-mini", openAiApiKey, serviceId: "openai"),
            new (AIProviderEnum.OpenAI, "text-embedding-3-small", openAiApiKey, serviceId: "openai", serviceType: KernelServiceEnum.EmbeddingGenerator),
            new (AIProviderEnum.GoogleGemini, "gemini-2.0-flash", geminiApiKey, serviceId: "gemini"),
            new (AIProviderEnum.GoogleGemini, "gemini-embedding-001", geminiApiKey, serviceId: "gemini", serviceType: KernelServiceEnum.EmbeddingGenerator),
        };
        var kernel = _semanticKernelManager.CreateKernel(configs);
        return kernel.IsError ? null : kernel.Value;

    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
