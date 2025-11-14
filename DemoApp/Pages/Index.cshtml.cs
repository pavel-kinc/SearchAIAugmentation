using DemoApp.Models;
using DemoApp.Services.Interfaces;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PromptEnhancer.KnowledgeBase;
using PromptEnhancer.KnowledgeRecord;
using PromptEnhancer.KnowledgeSearchRequest.Examples;
using PromptEnhancer.Models;
using PromptEnhancer.Models.Configurations;
using PromptEnhancer.Models.Examples;
using PromptEnhancer.Pipeline.Interfaces;
using PromptEnhancer.Pipeline.PromptEnhancerSteps;
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

        [TempData]
        public string? FloatingAlertMessage { get; set; }

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
            FloatingAlertMessage = "Kernel Configuration Saved";
            return Page();
        }

        public IActionResult OnPostUpdateSearchConf()
        {
            _configurationService.UpdateSearchConfig(ViewModel.ConfigurationSetup.SearchConfiguration);
            FloatingAlertMessage = "Search Configuration Saved";
            return Page();
        }

        public IActionResult OnPostUpdatePromptConf()
        {
            _configurationService.UpdatePromptConfig(ViewModel.ConfigurationSetup.PromptConfiguration);
            FloatingAlertMessage = "Prompt Configuration Saved";
            return Page();
        }

        public IActionResult OnPostUpdateEntries()
        {
            _entrySetupService.UpdateEntries(Entries);
            FloatingAlertMessage = "Entries Updated";
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
            //TODO figure out the config
            //_configurationService.ClearSession();
            var appConfig = _configurationService.GetConfiguration(true);
            var entries = _entrySetupService.GetEntries();
            if (entries.Any(x => string.IsNullOrWhiteSpace(x.QueryString)))
            {
                for (int i = 0; i < entries.Count(); i++)
                {
                    if (string.IsNullOrWhiteSpace(entries.ElementAt(i).QueryString))
                    {
                        ViewModel.Errors.Add($"{i + 1}. entry has empty query.");
                    }
                }
                return Page();
            }

            EnhancerConfiguration enhancerConfig = GetEnhancerConfiguration(appConfig);
            var res = await _enhancerService.ProcessConfiguration(enhancerConfig, entries);
            if (!res.IsError)
            {
                //TODO if it is error, show message
                ViewModel.ResultModelList = res.Value;
            }
            else
            {
                ViewModel.Errors = res.Errors.Select(x => x.Code).ToList();
                return Page();
            }
            return Page();
        }

        private static EnhancerConfiguration GetEnhancerConfiguration(ConfigurationSetup appConfig)
        {
            var enhancerConfig = appConfig.Adapt<EnhancerConfiguration>();

            var request = new GoogleSearchRequest
            {
                Settings = new GoogleSettings
                {
                    SearchApiKey = appConfig.SearchConfiguration.SearchProviderSettings.SearchApiKey!,
                    Engine = appConfig.SearchConfiguration.SearchProviderSettings.Engine!,
                },
                Filter = appConfig.SearchConfiguration.SearchFilter
            };

            //TODO defensive copy in lib? (and also of settings and such)
            enhancerConfig.Steps = new List<IPipelineStep>
                {
                    new PreprocessStep(),
                    new KernelContextPluginsStep(),
                    new QueryParserStep(),
                    //TODO automatic step picker calling? picking from list of steps(defined by user)/bases(from inject pick bases)/each has its own call to llm if it is viable for the given query(steps defined from user so kinda like 1.)
                    new SearchStep<KnowledgeUrlRecord, GoogleSearchFilterModel, GoogleSettings, UrlRecordFilter, UrlRecord>(request)
                };
            return enhancerConfig;
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
