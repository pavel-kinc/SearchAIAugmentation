using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel;
using PromptEnhancer.Extensions;
using PromptEnhancer.KnowledgeBaseCore;
using PromptEnhancer.KnowledgeBaseCore.Examples;
using PromptEnhancer.KnowledgeBaseCore.Interfaces;
using PromptEnhancer.KnowledgeRecord;
using PromptEnhancer.KnowledgeSearchRequest.Examples;
using PromptEnhancer.Models.Examples;
using TaskChatDemo.KnowledgeBases;
using TaskChatDemo.Models.TaskItem;
using TaskChatDemo.Services.ApiConsumer;
using TaskChatDemo.Services.VectorStore;

namespace TaskChatDemo;

public class Program
{
    public async static Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.AddServiceDefaults();

        builder.Configuration.AddJsonFile("appsettings.secrets.json", optional: false, reloadOnChange: true);

        // Add services to the container.
        builder.Services.AddControllersWithViews();
        builder.Services.AddInMemoryVectorStore();

        builder.Services.AddSingleton(sp =>
        {
            var vectorStore = sp.GetRequiredService<VectorStore>();
            var collection = vectorStore.GetCollection<Guid, TaskItemModel>("tasks");
            // Ensure at registration time that collection exists
            collection.EnsureCollectionExistsAsync().GetAwaiter().GetResult();
            return collection;
        });

        builder.Services.AddDataProtection();

        builder.Services.AddSession(options =>
        {
            options.IdleTimeout = TimeSpan.FromMinutes(10);
            options.Cookie.Name = ".Chat.Session";
            options.Cookie.IsEssential = true;
        });


        builder.Services.AddSingleton<IVectorStoreService, VectorStoreService>();
        builder.Services.AddSingleton<IWorkItemApiService, WorkItemApiService>();

        builder.Services.AddPromptEnhancer();
        builder.Services.AddSingleton<TaskDataKnowledgeBase, TaskDataKnowledgeBase>();
        builder.Services.AddSingleton<WorkItemKnowledgeBase, WorkItemKnowledgeBase>();

        var app = builder.Build();

        var path = Path.Combine(builder.Environment.ContentRootPath, "Data", "task_models.json");
        var taskModels = TaskItemModelUtility.LoadFromJson(path);

        var vectorStore = app.Services.GetRequiredService<VectorStore>();
        var taskCollection = vectorStore.GetCollection<Guid, TaskItemModel>("tasks");
        await taskCollection.EnsureCollectionExistsAsync();
        await taskCollection.UpsertAsync(taskModels);

        app.MapDefaultEndpoints();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }
        app.UseSession();

        app.UseHttpsRedirection();
        app.UseRouting();

        app.UseAuthorization();

        app.MapStaticAssets();
        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}")
            .WithStaticAssets();

        app.Run();
    }
}
