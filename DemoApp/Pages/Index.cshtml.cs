using DemoApp.Models;
using DemoApp.Services.Interfaces;
using Mapster;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PromptEnhancer.Models;
using PromptEnhancer.Models.Configurations;
using PromptEnhancer.Services.EnhancerService;

namespace DemoApp.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        private readonly IConfigurationSetupService _configurationService;
        private readonly IEntrySetupService _entrySetupService;
        private readonly IEnhancerService _enhancerService;

        [BindProperty]
        public EnhancerViewModel ViewModel { get; set; } = new();
        [BindProperty]
        public List<Entry> Entries { get; set; } = [];

        public IndexModel(ILogger<IndexModel> logger, IConfiguration configuration, IConfigurationSetupService configurationService, IEnhancerService enhancerService, IEntrySetupService entrySetupService)
        {
            _logger = logger;
            _configurationService = configurationService;
            _enhancerService = enhancerService;
            _entrySetupService = entrySetupService;
        }

        public void OnGet()
        {
            var entries = _entrySetupService.GetEntries();
            if (!entries.Any())
            {
                _entrySetupService.AddEntry(new Entry());
            }
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

        public IActionResult OnPostUpdateEntries()
        {
            _entrySetupService.UpdateEntries(Entries);
            return Page();
        }

        public IActionResult OnPostAddEntry()
        {
            _entrySetupService.AddEntry(new Entry());
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
            //_configurationService.ClearSession();
            var config = _configurationService.GetConfiguration(true).Adapt<EnhancerConfiguration>();
            var entries = _entrySetupService.GetEntries();
            var res = await _enhancerService.ProcessConfiguration(config, entries);
            if (!res.IsError)
            {
                //TODO if it is error, show message
                ViewModel.ResultModelList = res.Value;
            }
            //ViewModel.ResultModelList = [new(), new(), new(), new(), new(), new(), new(), new(), new(), new()];
            return Page();
        }

        public IActionResult OnPostClearSession()
        {
            _configurationService.ClearSession();
            _entrySetupService.AddEntry(new Entry());
            return Page();
        }

        public override void OnPageHandlerExecuted(PageHandlerExecutedContext context)
        {
            base.OnPageHandlerExecuted(context);

            ViewModel.ConfigurationSetup = _configurationService.GetConfiguration();
            Entries = _entrySetupService.GetEntries().ToList();
        }
    }
}
