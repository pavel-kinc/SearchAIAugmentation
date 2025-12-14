using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.AI;
using PromptEnhancer.Models;
using PromptEnhancer.Models.Pipeline;
using PromptEnhancer.Services.ChatHistoryService;
using PromptEnhancer.Services.EnhancerService;
using System.Diagnostics;
using System.Text.Json;
using TaskChatDemo.Models;
using TaskChatDemo.Services.EnhancerUtility;

namespace TaskChatDemo.Controllers;

/// <summary>
/// Represents the main controller for handling requests related to the application's home page, chat
/// functionality, and session management.
/// </summary>
/// <remarks>This controller provides endpoints for rendering the home and privacy views, managing user sessions,
/// and handling chat-related operations, including streaming responses. It relies on dependency-injected services for
/// logging, configuration, and enhancing chat functionality.</remarks>
public class HomeController : Controller
{
    private const string ChatHistory = "EntryKey";
    private readonly ILogger<HomeController> _logger;
    private readonly IEnhancerService _enhancerService;
    private readonly IEnhancerUtilityService _enhancerUtilityService;
    private readonly IChatHistoryService _chatHistoryService;

    public HomeController(IEnhancerService enhancerService, ILogger<HomeController> logger, IEnhancerUtilityService enhancerUtilityService, IChatHistoryService chatHistoryService)
    {
        _enhancerService = enhancerService;
        _logger = logger;
        _enhancerUtilityService = enhancerUtilityService;
        _chatHistoryService = chatHistoryService;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [HttpPost("/session/clear")]
    [ValidateAntiForgeryToken]
    public IActionResult ClearSession()
    {
        HttpContext.Session.Clear();
        _logger.LogInformation("Session cleared");
        return Json(new { success = true });
    }


    /// <summary>
    /// Handles a streaming chat interaction, processing the query and returning the result asynchronously.
    /// </summary>
    /// <remarks>This method processes the chat query by retrieving and updating the chat history stored in
    /// the session.  It integrates with a pipeline to enhance the query context and handles streaming messages.  If an
    /// error occurs during processing, the method logs the error and returns a bad request response.</remarks>
    /// <param name="q">The query string representing the user's input to the chat.</param>
    /// <param name="skipPipeline">A boolean value indicating whether to bypass the pipeline processing.  <see langword="true"/> to skip the
    /// pipeline; otherwise, <see langword="false"/>.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> used to cancel the operation if needed.</param>
    /// <returns>An <see cref="IActionResult"/> representing the result of the chat operation.  Returns an empty result on
    /// success, or a <see cref="BadRequestObjectResult"/> if an error occurs.</returns>
    [HttpGet("/chat/stream")]
    public async Task<IActionResult> Chat(string q, bool skipPipeline = false, CancellationToken ct = default)
    {
        try
        {
            var chatHistory = HttpContext.Session.GetString(ChatHistory) is string json
                ? JsonSerializer.Deserialize<List<ChatMessage>>(json)
                : null;
            HttpContext.Session.SetString(ChatHistory, JsonSerializer.Serialize(chatHistory));
            var entry = new Entry() { QueryString = q };

            var settingsResult = _enhancerUtilityService.GetPipelineSettings();
            if (settingsResult.IsError)
            {
                _logger.LogInformation("Failed to get pipeline settings: {Error}", settingsResult.FirstError);
                return BadRequest(settingsResult.FirstError);
            }
            var settings = settingsResult.Value;

            var context = await _enhancerUtilityService.GetContextFromPipeline(q, skipPipeline, entry, settings);
            context.ChatHistory ??= chatHistory;
            _logger.LogInformation("Starting streaming response for query: {Query}", q);
            chatHistory = await HandleStreamingMessage(chatHistory, settings, context, ct);
            HttpContext.Session.SetString(ChatHistory, JsonSerializer.Serialize(chatHistory));
            return new EmptyResult();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed streaming");
            return BadRequest(ex);
        }

    }

    /// <summary>
    /// Processes a streaming chat message, updates the chat history, and sends real-time updates to the client.
    /// </summary>
    /// <remarks>This method streams real-time updates to the client using the Server-Sent Events (SSE)
    /// protocol. Each update is sent as a data event, and a final "done" event is sent to indicate the end of the
    /// stream. The method aggregates the updates into a chat response and appends the resulting messages to the chat
    /// history.</remarks>
    /// <param name="chatHistory">The existing chat history. Can be <see langword="null"/> if no prior history exists.</param>
    /// <param name="settings">The pipeline settings used to configure the streaming response behavior.</param>
    /// <param name="context">The pipeline run context, which includes state and metadata for the current operation.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> to observe while waiting for the operation to complete.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the updated chat history as a list
    /// of <see cref="ChatMessage"/> objects.</returns>
    private async Task<List<ChatMessage>> HandleStreamingMessage(List<ChatMessage>? chatHistory, PipelineSettings settings, PipelineRun context, CancellationToken ct)
    {
        var res = _enhancerService.GetStreamingResponse(settings, context, ct);
        List<ChatResponseUpdate> updates = [];
        Response.ContentType = "text/event-stream";
        await foreach (ChatResponseUpdate update in res)
        {
            await Response.StartAsync(ct);
            await Response.WriteAsync($"data: {update}\n\n", ct);
            await Response.Body.FlushAsync(ct);
            updates.Add(update);
        }

        await Response.WriteAsync("event: done\ndata: end\n\n", ct);
        await Response.Body.FlushAsync(ct);
        chatHistory = context.ChatHistory?.ToList() ?? [];
        var chatResponse = updates.ToChatResponse();
        // add output token usage, input is added in streaming response
        _chatHistoryService.AddTokenUsageToPipelineRunContext(context, [], response: chatResponse);
        chatHistory.AddRange(chatResponse.Messages);
        return chatHistory;
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
