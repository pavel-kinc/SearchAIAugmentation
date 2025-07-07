using DemoApp.Models;
using DemoApp.Services;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using PromptEnhancer.Models;
using PromptEnhancer.Models.Configurations;
using PromptEnhancer.Search;
using PromptEnhancer.SK;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Filters;
using PromptEnhancer.Services;

namespace DemoApp.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        private readonly IConfigurationSetupService _configurationService;
        private readonly IEnhancerService _enhancerService;

        [BindProperty]
        public EnhancerConfiguratorViewModel ViewModel { get; set; } = new();

        private int number = 0;

        public IndexModel(ILogger<IndexModel> logger, IConfiguration configuration, IConfigurationSetupService configurationService, IEnhancerService enhancerService)
        {
            _logger = logger;
            _configurationService = configurationService;
            _enhancerService = enhancerService;
        }

        public void OnGet()
        {
           number++;
        }

        public IActionResult OnPostUpdateKernelConf()
        {
            _configurationService.UpdateKernelConfig(ViewModel.ConfigurationSetup.KernelConfiguration);
            return Page();
        }

        public IActionResult OnPostUpdateSearchConf()
        {
            _configurationService.UpdateSearchConfig(ViewModel.ConfigurationSetup.SearchConfiguration);
            return Page();
        }

        public IActionResult OnPostUpdatePromptConf()
        {
            _configurationService.UpdatePromptConfig(ViewModel.ConfigurationSetup.PromptConfiguration);
            return Page();
        }

        public IActionResult OnPostDownloadConfiguration()
        {
            var config = _configurationService.GetConfiguration().Adapt<EnhancerConfiguration>();
            var bytes = _enhancerService.ExportConfigurationToBytes(config);
            return File(bytes, "application/json", "enhancer_config.json");
        }

        public async Task<IActionResult> OnPostUpload(IFormFile configFile)
        {
            await using var ms = new MemoryStream();
            await configFile.CopyToAsync(ms);

            var config = _enhancerService.ImportConfigurationFromBytes(ms.ToArray());
            if (config is not null)
            {
                _configurationService.UploadConfiguration(config.Adapt<ConfigurationSetup>());
            }

            return Page();
        }

        public async Task<IActionResult> OnPostProcessResultModel()
        {
            var config = _configurationService.GetConfiguration(true).Adapt<EnhancerConfiguration>();
            ViewModel.ResultModel = await _enhancerService.ProcessConfiguration(config);
            return Page();
        }

        public override void OnPageHandlerExecuted(PageHandlerExecutedContext context)
        {
            base.OnPageHandlerExecuted(context);

            ViewModel.ConfigurationSetup = _configurationService.GetConfiguration();
        }
    }
}
