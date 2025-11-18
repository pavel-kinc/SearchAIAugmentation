using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;
using PromptEnhancer.Models;
using PromptEnhancer.Models.Pipeline;
using PromptEnhancer.Services.EnhancerService;
using PromptEnhancer.SK.Interfaces;
using System.Diagnostics;
using System.Text.Json;
using TaskChatDemo.Models;
using TaskChatDemo.Models.TaskItem;
using TaskChatDemo.Services.ApiConsumer;

namespace TaskChatDemo.Controllers;

public class HomeController : Controller
{
    private const string EntrySessionKey = "EntryKey";
    private readonly ILogger<HomeController> _logger;
    private readonly IEnhancerService _enhancerService;
    private readonly IConfiguration _configuration;
    private readonly ISemanticKernelManager _semanticKernelManager;
    private readonly IServiceProvider _serviceProvider;
    private readonly VectorStoreCollection<Guid, TaskItemModel> _taskCollection;
    private readonly IWorkItemApiService _workItemApiService;

    public HomeController(IEnhancerService enhancerService, ILogger<HomeController> logger, IConfiguration configuration, ISemanticKernelManager skManager, IServiceProvider serviceProvider, VectorStoreCollection<Guid, TaskItemModel> taskCollection, IWorkItemApiService workItemApiService)
    {
        _enhancerService = enhancerService;
        _logger = logger;
        _configuration = configuration;
        _semanticKernelManager = skManager;
        _serviceProvider = serviceProvider;
        _taskCollection = taskCollection;
        _workItemApiService = workItemApiService;
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
    public async Task<IActionResult> Chat(string q, bool skipPipeline = false, CancellationToken ct = default)
    {
        var entry = HttpContext.Session.GetString(EntrySessionKey) is string json
                ? JsonSerializer.Deserialize<Entry>(json)
                : null;
        entry ??= new Entry() { QueryString = q };
        var enhancerConfig = _enhancerService.CreateDefaultConfiguration(aiApiKey: _configuration["AIServices:OpenAI:ApiKey"]);
        var settings = _enhancerService.CreatePipelineSettingsFromConfig(enhancerConfig.PromptConfiguration, enhancerConfig.PipelineAdditionalSettings, enhancerConfig.KernelConfiguration).Value;

        if (true /*skipPipeline*/)
        {

        }
        else
        {

        }


        var context = new PipelineContext()
        {
            UserPromptToLLM = q,
        };
        //var smth1 = await _workItemApiService.GetWorkItemsAsync(new SearchWorkItemFilterModel(), new WorkItemSearchSettings().ApiUrl);
        var generator = settings.Kernel.GetRequiredService<IEmbeddingGenerator<string, Embedding<float>>>();
        var queryVec = await generator.GenerateAsync(q);
        var results = await _taskCollection.SearchAsync(queryVec.Vector, top: 7).ToListAsync();
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
        var smth = updates.ToChatResponse();
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
