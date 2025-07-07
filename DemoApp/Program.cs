using DemoApp.Services;

using PromptEnhancer.Extensions;

namespace DemoApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.AddServiceDefaults();

            builder.Configuration.AddJsonFile("appsettings.secrets.json", optional: false, reloadOnChange: true);

            // Add services to the container.
            builder.Services.AddRazorPages();

            builder.Services.AddSingleton<IConfigurationSetupService, ConfigurationSetupService>();

            builder.Services.AddPromptEnhancer();

            var app = builder.Build();

            app.MapDefaultEndpoints();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapRazorPages();

            app.Run();
        }
    }
}
