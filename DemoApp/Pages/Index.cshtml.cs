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

namespace DemoApp.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        private readonly IConfigurationSetupService _configurationService;

        [BindProperty]
        public EnhancerConfiguratorViewModel ViewModel { get; set; } = new();

        private int number = 0;

        public IndexModel(ILogger<IndexModel> logger, IConfiguration configuration, IConfigurationSetupService configurationService)
        {
            _logger = logger;
            _configurationService = configurationService;
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
            throw new NotImplementedException();
            //return Page();
        }

        public async Task<IActionResult> OnPostProcessResultModel()
        {
            var config = _configurationService.GetConfiguration().Adapt<EnhancerConfiguration>();
            ViewModel.ResultModel = await SemanticKernelManager.ProcessConfiguration(config);
            return Page();
        }

        public override void OnPageHandlerExecuted(PageHandlerExecutedContext context)
        {
            base.OnPageHandlerExecuted(context);

            ViewModel.ConfigurationSetup = _configurationService.GetConfiguration();
        }
    }
}
