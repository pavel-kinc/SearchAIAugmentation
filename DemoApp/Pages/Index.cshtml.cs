using DemoApp.Models;
using DemoApp.Services.Interfaces;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel;
using PromptEnhancer.KnowledgeBaseCore;
using PromptEnhancer.KnowledgeBaseCore.Examples;
using PromptEnhancer.KnowledgeRecord;
using PromptEnhancer.KnowledgeSearchRequest.Examples;
using PromptEnhancer.Models;
using PromptEnhancer.Models.Configurations;
using PromptEnhancer.Models.Examples;
using PromptEnhancer.Models.Pipeline;
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
        private readonly GoogleKnowledgeBase _googleKB;
        private readonly IEnhancerService _enhancerService;

        [BindProperty]
        public EnhancerViewModel ViewModel { get; set; } = new();
        [BindProperty]
        public List<Entry> Entries { get; set; } = [];


        public IndexModel(ILogger<IndexModel> logger, IConfiguration configuration, IConfigurationSetupService configurationService, IEnhancerService enhancerService, IEntrySetupService entrySetupService, GoogleKnowledgeBase googleKB)
        {
            _logger = logger;
            _configurationService = configurationService;
            _enhancerService = enhancerService;
            _entrySetupService = entrySetupService;
            _googleKB = googleKB;
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

        private EnhancerConfiguration GetEnhancerConfiguration(ConfigurationSetup appConfig)
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

            var container = new KnowledgeBaseContainer<KnowledgeUrlRecord, GoogleSearchFilterModel, GoogleSettings, UrlRecordFilter, UrlRecord>(_googleKB, request, null);
            enhancerConfig.PipelineAdditionalSettings = AssignExecutionSettingsAndOptions(enhancerConfig.PipelineAdditionalSettings, appConfig.GenerationConfiguration);

            //TODO defensive copy in lib? (and also of settings and such)
            enhancerConfig.Steps = new List<IPipelineStep>
                {
                    new PreprocessStep(),
                    new KernelContextPluginsStep(),
                    new QueryParserStep(),
                    //new SearchStep<KnowledgeUrlRecord, GoogleSearchFilterModel, GoogleSettings, UrlRecordFilter, UrlRecord>(request),
                    new MultipleSearchStep([container], allowAutoChoice: false, isRequired: true),
                    new ProcessEmbeddingStep(skipGenerationForEmbData: true, isRequired: true),
                    new ProcessRankStep(isRequired: true),
                    new ProcessFilterStep(new RecordPickerOptions(){MinScoreSimilarity = 0.3d, Take = 5, OrderByScoreDescending = true}, isRequired: true),
                    new PostProcessCheckStep(),
                    new PromptBuilderStep(isRequired: true),
                    new GenerationStep(isRequired: true),
                };
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
                ChatClientKey = pipelineAdditionalSettings.ChatClientKey
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
