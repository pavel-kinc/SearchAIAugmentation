using DemoApp.Models;
using DemoApp.Services.Interfaces;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel;
using Newtonsoft.Json;
using PromptEnhancer.CustomJsonResolver;
using PromptEnhancer.Models;
using PromptEnhancer.Models.Configurations;
using PromptEnhancer.Models.Pipeline;
using PromptEnhancer.Services.EnhancerService;
using System.Text;

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
            if (ModelState.IsValid)
            {
                _configurationService.UpdateKernelConfig(ViewModel.ConfigurationSetup.KernelConfiguration);
                ViewModel.FloatingAlertMessage = "Kernel Configuration Saved";
            }
            return Page();
        }

        public IActionResult OnPostUpdateSearchConf()
        {
            if (ModelState.IsValid)
            {
                _configurationService.UpdateSearchConfig(ViewModel.ConfigurationSetup.SearchConfiguration);
                ViewModel.FloatingAlertMessage = "Search Configuration Saved";
            }
            return Page();
        }

        public IActionResult OnPostUpdatePromptConf()
        {
            if (ModelState.IsValid)
            {
                _configurationService.UpdatePromptConfig(ViewModel.ConfigurationSetup.PromptConfiguration);
                ViewModel.FloatingAlertMessage = "Prompt Configuration Saved";
            }
            return Page();
        }
        public IActionResult OnPostUpdateGenerationConf()
        {
            if (ModelState.IsValid)
            {
                _configurationService.UpdateGenerationConfig(ViewModel.ConfigurationSetup.GenerationConfiguration);
                ViewModel.FloatingAlertMessage = "Generation Configuration Saved";
            }
            return Page();
        }

        public IActionResult OnPostUpdateEntries()
        {
            if (ModelState.IsValid)
            {
                _entrySetupService.UpdateEntries(Entries);
                ViewModel.FloatingAlertMessage = "Entries Updated";
            }
            return Page();
        }

        public IActionResult OnPostAddEntry()
        {
            if (ModelState.IsValid)
            {
                _entrySetupService.AddEntry(new Entry());
            }
            return Page();
        }

        public IActionResult OnPostDownloadConfiguration()
        {
            var config = _configurationService.GetConfiguration();
            var json = GetConfigurationJson(config, true);
            var bytes = Encoding.UTF8.GetBytes(json);
            return File(bytes, "application/json", "enhancer_config.json");
        }

        public async Task<IActionResult> OnPostUpload(IFormFile configFile)
        {
            await using var ms = new MemoryStream();
            await configFile.CopyToAsync(ms);

            var json = Encoding.UTF8.GetString(ms.ToArray());
            var config = JsonConvert.DeserializeObject<ConfigurationSetup>(json);
            //var config = _enhancerService.ImportConfigurationFromBytes(ms.ToArray());
            if (config is not null)
            {
                _configurationService.UploadConfiguration(config);
            }

            return Page();
        }

        public async Task<IActionResult> OnPostProcessResultModel()
        {
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
            if (!res.IsError && res.Value.Any())
            {
                ViewModel.ResultModelList = res.Value;
            }
            else if (!res.IsError && !res.Value.Any())
            {
                ViewModel.Errors.Add("Error: No results returned from the processing!");
            }
            else
            {
                ViewModel.Errors = [.. res.Errors.Select(x => x.Code)];
            }
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

        private EnhancerConfiguration GetEnhancerConfiguration(ConfigurationSetup appConfig)
        {
            var enhancerConfig = appConfig.Adapt<EnhancerConfiguration>();

            var apiKey = appConfig.SearchConfiguration.SearchProviderSettings.SearchApiKey!;
            var engine = appConfig.SearchConfiguration.SearchProviderSettings.Engine!;
            var searchFilter = appConfig.SearchConfiguration.SearchFilter;

            enhancerConfig.PipelineAdditionalSettings = AssignExecutionSettingsAndOptions(enhancerConfig.PipelineAdditionalSettings, appConfig.GenerationConfiguration);

            enhancerConfig.Steps = _enhancerService.CreateDefaultGoogleSearchPipelineSteps(apiKey, engine, searchFilter);
            return enhancerConfig;
        }

        private static PipelineAdditionalSettings AssignExecutionSettingsAndOptions(PipelineAdditionalSettings pipelineAdditionalSettings, GenerationConfiguration generationConfiguration)
        {
            var chatOptions = new ChatOptions
            {
                Temperature = generationConfiguration.Temperature,
                MaxOutputTokens = generationConfiguration.MaxTokens,
                TopP = generationConfiguration.TopP,
                FrequencyPenalty = generationConfiguration.FrequencyPenalty,
                PresencePenalty = generationConfiguration.PresencePenalty
            };
            PromptExecutionSettings executionSettings = GetExecutionSettings(generationConfiguration);

            return new PipelineAdditionalSettings
            {
                ChatOptions = chatOptions,
                KernelRequestSettings = executionSettings,
                GeneratorKey = pipelineAdditionalSettings.GeneratorKey,
                ChatClientKey = pipelineAdditionalSettings.ChatClientKey,
                MaximumInputLength = generationConfiguration.PromptSizeLimit,
            };
        }

        private static PromptExecutionSettings GetExecutionSettings(GenerationConfiguration generationConfiguration)
        {
            var promptSettings = new PromptExecutionSettings();
            if (promptSettings.ExtensionData is null)
            {
                promptSettings.ExtensionData = new Dictionary<string, object>();
            }

            promptSettings.ExtensionData["max_tokens"] = generationConfiguration.MaxTokens;

            if (generationConfiguration.Temperature.HasValue)
                promptSettings.ExtensionData["temperature"] = generationConfiguration.Temperature.Value;

            if (generationConfiguration.TopP.HasValue)
                promptSettings.ExtensionData["top_p"] = generationConfiguration.TopP.Value;

            if (generationConfiguration.FrequencyPenalty.HasValue)
                promptSettings.ExtensionData["frequency_penalty"] = generationConfiguration.FrequencyPenalty.Value;

            if (generationConfiguration.PresencePenalty.HasValue)
                promptSettings.ExtensionData["presence_penalty"] = generationConfiguration.PresencePenalty.Value;
            if (generationConfiguration.AllowFunctionCalling)
            {
                promptSettings.FunctionChoiceBehavior = FunctionChoiceBehavior.Auto();
            }

            return promptSettings;
        }

        private string GetConfigurationJson(ConfigurationSetup configuration, bool hideSecrets = true)
        {
            var settings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented
            };

            if (hideSecrets)
            {
                settings.ContractResolver = new SensitiveContractResolver();
            }

            return JsonConvert.SerializeObject(configuration, settings);
        }
    }
}
