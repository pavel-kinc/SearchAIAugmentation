using ErrorOr;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel;
using PromptEnhancer.Models;
using PromptEnhancer.Models.Pipeline;
using PromptEnhancer.Services.EnhancerService;
using System.Diagnostics;
using System.Text.Json;
using TaskChatDemo.Models;
using TaskChatDemo.Services.EnhancerUtility;

namespace TaskChatDemo.Controllers;

public class HomeController : Controller
{
    private const string ChatHistory = "EntryKey";
    private readonly ILogger<HomeController> _logger;
    private readonly IEnhancerService _enhancerService;
    private readonly IConfiguration _configuration;
    private readonly IEnhancerUtilityService _enhancerUtilityService;

    public HomeController(IEnhancerService enhancerService, ILogger<HomeController> logger, IConfiguration configuration, IEnhancerUtilityService enhancerUtilityService)
    {
        _enhancerService = enhancerService;
        _logger = logger;
        _configuration = configuration;
        _enhancerUtilityService = enhancerUtilityService;
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

        var settingsResult = _enhancerUtilityService.GetPipelineSettings();
        if (settingsResult.IsError)
        {
            //return RedirectToAction("Error");
        }
        var settings = settingsResult.Value;

        var context = await _enhancerUtilityService.GetContextFromPipeline(q, skipPipeline, entry, settings);
        context.ChatHistory ??= chatHistory;

        chatHistory = await HandleStreamingMessage(chatHistory, settings, context, ct);
        HttpContext.Session.SetString(ChatHistory, JsonSerializer.Serialize(chatHistory));
        return;
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

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
