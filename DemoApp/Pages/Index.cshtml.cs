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
    /// <summary>
    /// Represents the page model for the Index page, providing functionality for managing configurations, entries, and
    /// processing results within the application.
    /// </summary>
    /// <remarks>This class handles various operations such as retrieving and updating configurations,
    /// managing entries, and processing results using the enhancer service. It also provides methods for uploading and
    /// downloading configuration files, as well as clearing session data. The page model interacts with multiple
    /// services to perform these operations, including logging, configuration setup, entry setup, and enhancement
    /// services.</remarks>
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

        /// <summary>
        /// Executes the pipeline based on the current configuration and entry setup.
        /// </summary>
        /// <remarks>This method validates the entries to ensure that none of them have an empty query
        /// string.  If validation fails, error messages are added to the view model and the page is returned. If
        /// validation succeeds, the method processes the configuration and entries using the enhancer service. The
        /// results or any processing errors are then added to the view model.</remarks>
        /// <returns>A <see cref="Task{IActionResult}"/> representing the asynchronous operation.  Returns the current page with
        /// updated view model data, including results or error messages.</returns>
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

        /// <summary>
        /// Executes after a page handler method has been invoked.
        /// </summary>
        /// <remarks>This method updates the <c>ViewModel.ConfigurationSetup</c> with the current
        /// configuration and populates the <c>Entries</c> collection with the latest entries.</remarks>
        /// <param name="context">The <see cref="PageHandlerExecutedContext"/> containing information about the current request and response.</param>
        public override void OnPageHandlerExecuted(PageHandlerExecutedContext context)
        {
            base.OnPageHandlerExecuted(context);
            ViewModel.ConfigurationSetup = _configurationService.GetConfiguration();
            Entries = _entrySetupService.GetEntries().ToList();
        }

        /// <summary>
        /// Retrieves the enhancer configuration based on the provided application configuration.
        /// </summary>
        /// <remarks>This method adapts the provided <paramref name="appConfig"/> to an <see
        /// cref="EnhancerConfiguration"/> and initializes it with default pipeline steps for Google Search using the
        /// specified API key, engine, and search filter.</remarks>
        /// <param name="appConfig">The application configuration containing settings for search and generation.</param>
        /// <returns>An <see cref="EnhancerConfiguration"/> object configured with the necessary pipeline steps and settings.</returns>
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

        /// <summary>
        /// Assigns execution settings and options to a <see cref="PipelineAdditionalSettings"/> instance based on the
        /// provided configuration.
        /// </summary>
        /// <param name="pipelineAdditionalSettings">The existing settings to which execution settings and options will be assigned. Must not be null.</param>
        /// <param name="generationConfiguration">The configuration containing parameters for generation, such as temperature and token limits. Must not be
        /// null.</param>
        /// <returns>A new <see cref="PipelineAdditionalSettings"/> instance with updated execution settings and options.</returns>
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

        /// <summary>
        /// Configures and returns the execution settings for a prompt based on the specified generation configuration.
        /// </summary>
        /// <param name="generationConfiguration">The configuration settings that influence prompt execution, including token limits and behavioral
        /// parameters.</param>
        /// <returns>A <see cref="PromptExecutionSettings"/> object containing the execution parameters derived from the provided
        /// configuration.</returns>
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

        /// <summary>
        /// Serializes the specified configuration object to a JSON string.
        /// </summary>
        /// <param name="configuration">The configuration object to serialize.</param>
        /// <param name="hideSecrets">A value indicating whether sensitive information should be hidden in the serialized JSON. true to hide
        /// sensitive information; otherwise, false.</param>
        /// <returns>A JSON string representation of the configuration object.</returns>
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
