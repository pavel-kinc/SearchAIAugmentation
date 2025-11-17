using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.AI;
using Microsoft.ML.OnnxRuntimeGenAI;
using PromptEnhancer.Models.Pipeline;
using PromptEnhancer.Services.EnhancerService;
using PromptEnhancer.SK;
using PromptEnhancer.SK.Interfaces;
using System.Diagnostics;
using System.Text;
using TaskChatDemo.Models;

namespace TaskChatDemo.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IEnhancerService _enhancerService;
    private readonly IConfiguration _configuration;
    private readonly ISemanticKernelManager _semanticKernelManager;
    private readonly IServiceProvider _serviceProvider;

    public HomeController(IEnhancerService enhancerService, ILogger<HomeController> logger, IConfiguration configuration, ISemanticKernelManager skManager, IServiceProvider serviceProvider)
    {
        _enhancerService = enhancerService;
        _logger = logger;
        _configuration = configuration;
        _semanticKernelManager = skManager;
        _serviceProvider = serviceProvider;
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
    public async Task<IActionResult> StreamTest(string q, CancellationToken ct = default)
    {
        var enhancerConfig = _enhancerService.CreateDefaultConfiguration(aiApiKey: _configuration["AIServices:OpenAI:ApiKey"]);
        var createdKernel = _semanticKernelManager.CreateKernel(_semanticKernelManager.ConvertConfig(enhancerConfig.KernelConfiguration));
        var settings = new PipelineSettings(createdKernel.Value, _serviceProvider, enhancerConfig.PipelineAdditionalSettings, enhancerConfig.PromptConfiguration);
        var context = new PipelineContext()
        {
            UserPromptToLLM = q,
        };
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
