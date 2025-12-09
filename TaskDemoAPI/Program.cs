
using Microsoft.OpenApi.Models;
using TaskDemoAPI.Models;
using TaskDemoAPI.WorkItemRepository;

namespace TaskDemoAPI;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.AddServiceDefaults();

        //Demo purposes - streaming issues
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", policy =>
            {
                policy.AllowAnyOrigin()
                      .AllowAnyHeader()
                      .AllowAnyMethod();
            });
        });

        // Load WorkItems from JSON file
        var path = Path.Combine(builder.Environment.ContentRootPath, "Data", "workitems.json");
        var workItems = WorkItemSeedLoader.LoadFromJson(path);

        // Add services to the container.
        builder.Services.AddSingleton<IReadOnlyList<WorkItem>>(workItems);
        builder.Services.AddSingleton<IWorkItemRepository, WorkItemRepository.WorkItemRepository>();

        builder.Services.AddEndpointsApiExplorer();

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "WorkItem API",
                Version = "v1",
                Description = "Demo API with WorkItems and filtering"
            });
        });

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "WorkItem API V1");
                c.RoutePrefix = ""; // serve at root
            });
            //Demo purposes
            app.UseCors("AllowAll");
        }

        app.MapDefaultEndpoints();

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}
