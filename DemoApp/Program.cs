using DemoApp.Services;
using DemoApp.Services.Interfaces;
using Microsoft.AspNetCore.Localization;
using PromptEnhancer.Extensions;
using System.Globalization;

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
            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddDataProtection();
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromHours(2);
                options.Cookie.IsEssential = true;
            });
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddScoped<IConfigurationSetupService, ConfigurationSetupService>();
            builder.Services.AddScoped<IEntrySetupService, EntrySetupService>();

            builder.Services.AddPromptEnhancer();

            var app = builder.Build();

            app.MapDefaultEndpoints();

            //if (!app.Environment.IsDevelopment())
            //{
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
            //}

            var supportedCultures = new[] { new CultureInfo("en-US") };

            app.UseRequestLocalization(new RequestLocalizationOptions
            {
                DefaultRequestCulture = new RequestCulture("en-US"),
                SupportedCultures = supportedCultures,
                SupportedUICultures = supportedCultures
            });

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseSession();
            app.UseRouting();

            app.UseAuthorization();

            app.MapRazorPages();

            app.Run();
        }
    }
}
