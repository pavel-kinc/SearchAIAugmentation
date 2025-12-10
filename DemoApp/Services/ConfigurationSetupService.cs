using DemoApp.Models;
using DemoApp.Services.Interfaces;
using DemoApp.SessionUtility;
using Mapster;
using Newtonsoft.Json;
using PromptEnhancer.CustomJsonResolver;
using PromptEnhancer.Models.Configurations;
using PromptEnhancer.Models.Enums;
using PromptEnhancer.Services.EnhancerService;

namespace DemoApp.Services
{
    /// <summary>
    /// Provides functionality for managing and configuring application settings, including kernel, search, prompt, 
    /// generation, and demo application configurations. This service ensures that default configurations are set  and
    /// allows for updates to individual configuration components.
    /// </summary>
    /// <remarks>The <see cref="ConfigurationSetupService"/> interacts with the session state to persist
    /// configuration data  across requests. It initializes default configurations if none are present in the session
    /// and provides methods  to retrieve, update, and clear configurations. This service also supports handling
    /// sensitive data by optionally  excluding secrets when retrieving configurations.</remarks>
    public class ConfigurationSetupService : IConfigurationSetupService
    {
        private readonly IEnhancerService _enhancerService;
        private readonly ILogger<ConfigurationSetupService> _logger;
        private readonly IConfiguration _configuration;

        private readonly ISession _session;

        private const string SessionPrefix = "Config.";

        public ConfigurationSetupService(IConfiguration configuration, IEnhancerService enhancerService, IHttpContextAccessor ctx, ILogger<ConfigurationSetupService> logger)
        {

            _configuration = configuration;
            _enhancerService = enhancerService;
            _logger = logger;
            _session = ctx.HttpContext!.Session;
            if (!_session.Keys.Any(k => k.StartsWith(SessionPrefix)))
            {
                var config = GetDefaultConfiguration();
                SetConfigurationSetup(config);
            }
        }

        /// <inheritdoc/>
        public ConfigurationSetup GetConfiguration(bool withSecrets = false)
        {
            var config = new ConfigurationSetup
            {
                KernelConfiguration = _session.GetObjectFromJson<KernelConfiguration>(SessionPrefix + nameof(KernelConfiguration))!,
                SearchConfiguration = _session.GetObjectFromJson<SearchConfiguration>(SessionPrefix + nameof(SearchConfiguration))!,
                PromptConfiguration = _session.GetObjectFromJson<PromptConfiguration>(SessionPrefix + nameof(PromptConfiguration))!,
                GenerationConfiguration = _session.GetObjectFromJson<GenerationConfiguration>(SessionPrefix + nameof(GenerationConfiguration))!,
                DemoAppConfigSetup = _session.GetObjectFromJson<DemoAppConfigSetup>(SessionPrefix + nameof(DemoAppConfigSetup))!,
            };

            if (!withSecrets)
            {
                var serialized = JsonConvert.SerializeObject(config, new JsonSerializerSettings
                {
                    ContractResolver = new SensitiveContractResolver()
                });
                config = JsonConvert.DeserializeObject<ConfigurationSetup>(serialized);

            }
            _logger.LogInformation("Configuration retrieved from session");
            return config!;
        }

        public void UpdateKernelConfig(KernelConfiguration kernelConfiguration)
        {
            var kernelConfig = _session.GetObjectFromJson<KernelConfiguration>(SessionPrefix + nameof(KernelConfiguration));
            var demoAppConfig = _session.GetObjectFromJson<DemoAppConfigSetup>(SessionPrefix + nameof(DemoAppConfigSetup));

            demoAppConfig!.AIApiKeyFromInput = kernelConfiguration.AIApiKey is not null ? GetLoadedFromString(true, kernelConfig?.Provider.ToString()) : demoAppConfig!.AIApiKeyFromInput;

            kernelConfiguration.AIApiKey ??= kernelConfig!.AIApiKey;
            SetSessionPartialConfigAndDemoAppConfig(nameof(KernelConfiguration), kernelConfiguration, demoAppConfig);
        }

        public void UpdateSearchConfig(SearchConfiguration searchConfiguration)
        {
            var searchConfig = _session.GetObjectFromJson<SearchConfiguration>(SessionPrefix + nameof(SearchConfiguration));
            var demoAppConfig = _session.GetObjectFromJson<DemoAppConfigSetup>(SessionPrefix + nameof(DemoAppConfigSetup));

            demoAppConfig!.SearchApiKeyFromInput = searchConfiguration.SearchProviderSettings.SearchApiKey is not null ? GetLoadedFromString(true, searchConfig?.SearchProviderSettings.Provider.ToString()) : demoAppConfig.SearchApiKeyFromInput;
            demoAppConfig!.SearchEngineFromInput = searchConfiguration.SearchProviderSettings.Engine is not null ? GetLoadedFromString(true, searchConfig?.SearchProviderSettings.Provider.ToString()) : demoAppConfig.SearchEngineFromInput;

            searchConfiguration.SearchProviderSettings.SearchApiKey ??= searchConfig!.SearchProviderSettings.SearchApiKey;
            searchConfiguration.SearchProviderSettings.Engine ??= searchConfig!.SearchProviderSettings.Engine;
            SetSessionPartialConfigAndDemoAppConfig(nameof(SearchConfiguration), searchConfiguration, demoAppConfig);
        }

        public void UpdatePromptConfig(PromptConfiguration promptConfiguration)
        {
            //var promptConfig = _session.GetObjectFromJson<PromptConfiguration>(SessionPrefix + nameof(PromptConfiguration));
            _session.SetObjectAsJson(SessionPrefix + nameof(PromptConfiguration), promptConfiguration);
        }

        public void UpdateGenerationConfig(GenerationConfiguration generationConfiguration)
        {
            //var generationConfig = _session.GetObjectFromJson<GenerationConfiguration>(SessionPrefix + nameof(GenerationConfiguration));
            _session.SetObjectAsJson(SessionPrefix + nameof(GenerationConfiguration), generationConfiguration);
        }

        public void UpdateDemoAppConfig(DemoAppConfigSetup demoAppConfigSetup)
        {
            //var demoAppConfig = _session.GetObjectFromJson<DemoAppConfigSetup>(SessionPrefix + nameof(DemoAppConfigSetup));
            _session.SetObjectAsJson(SessionPrefix + nameof(SearchConfiguration), demoAppConfigSetup);
        }

        public void UploadConfiguration(ConfigurationSetup configuration)
        {
            SetDefaultDemoAppConfig(configuration);
            SetConfigurationSetup(configuration);
        }

        public void ClearSession()
        {
            _session.Clear();
            var config = GetDefaultConfiguration();
            SetConfigurationSetup(config);
        }

        /// <summary>
        /// Sets the session configuration for a specified configuration name and optionally updates the demo
        /// application configuration.
        /// </summary>
        /// <remarks>This method stores the provided <paramref name="partialConfig"/> in the session under
        /// a key derived from the specified <paramref name="configName"/>. If <paramref name="demoAppConfig"/> is
        /// provided, it is stored in the session under a predefined key.</remarks>
        /// <param name="configName">The name of the configuration to be stored in the session.</param>
        /// <param name="partialConfig">An object representing the partial configuration to be stored. This parameter cannot be null.</param>
        /// <param name="demoAppConfig">An optional object representing the demo application configuration. If null, no demo application
        /// configuration is updated.</param>
        public void SetSessionPartialConfigAndDemoAppConfig(string configName, object partialConfig, object? demoAppConfig)
        {
            _session.SetObjectAsJson(SessionPrefix + configName, partialConfig);
            if (demoAppConfig is not null)
            {
                _session.SetObjectAsJson(SessionPrefix + nameof(DemoAppConfigSetup), demoAppConfig);
            }
        }

        /// <summary>
        /// Creates and returns the default configuration setup for the application.
        /// </summary>
        /// <returns>A <see cref="ConfigurationSetup"/> object containing the default configuration settings.</returns>
        private ConfigurationSetup GetDefaultConfiguration()
        {
            var enhancerConfig = _enhancerService.CreateDefaultConfiguration(aiApiKey: _configuration["AIServices:OpenAI:ApiKey"]);
            var configSetup = enhancerConfig.Adapt<ConfigurationSetup>();
            configSetup.PromptConfiguration.SystemInstructions = """
                You are a professional e-commerce copywriter.
                Given the product defined by the user query, write a full, SEO-optimized product description.
                The output must contain exactly 5 paragraphs and nothing else, the first one should be general summary.
                You may use information from the provided context. If you do, you must include a citation right after each portion where you used that context.
                For the citation format use exactly: “~[url]” (only if you use data from it - url literal must be replace with real url).
                Do **not** promote, advertise, or encourage visiting the URL in any way.
                """;
            configSetup.PromptConfiguration.TargetOutputLength = 400;
            configSetup.SearchConfiguration.SearchProviderSettings = new SearchProviderSettings
            {
                SearchApiKey = _configuration["SearchConfigurations:Google:ApiKey"],
                Engine = _configuration["SearchConfigurations:Google:SearchEngineId"],
                Provider = SearchProviderEnum.Google,
            };

            SetDefaultDemoAppConfig(configSetup);
            _logger.LogInformation("Default configuration created");
            return configSetup;
        }

        /// <summary>
        /// Configures the default settings for the demo application based on the provided configuration setup.
        /// </summary>
        /// <param name="configSetup">The configuration setup object containing the necessary settings for the kernel, search provider, and demo
        /// application.</param>
        private void SetDefaultDemoAppConfig(ConfigurationSetup configSetup)
        {
            var searchProvider = configSetup.SearchConfiguration.SearchProviderSettings.Provider.ToString();
            configSetup.DemoAppConfigSetup.AIApiKeyFromInput = GetLoadedFromString(configSetup.KernelConfiguration.AIApiKey is not null ? false : null, configSetup.KernelConfiguration.Provider.ToString());
            configSetup.DemoAppConfigSetup.SearchApiKeyFromInput = GetLoadedFromString(configSetup.SearchConfiguration.SearchProviderSettings.SearchApiKey is not null ? false : null, searchProvider);
            configSetup.DemoAppConfigSetup.SearchEngineFromInput = GetLoadedFromString(configSetup.SearchConfiguration.SearchProviderSettings.Engine is not null ? false : null, searchProvider);
        }

        /// <summary>
        /// Configures the session with the specified setup values.
        /// </summary>
        /// <remarks>This method serializes the configuration properties of the provided <paramref
        /// name="configSetup"/> object  and stores them in the session using predefined keys. Each configuration
        /// property is stored as JSON.</remarks>
        /// <param name="configSetup">An instance of <see cref="ConfigurationSetup"/> containing the configuration values to be stored in the
        /// session.</param>
        private void SetConfigurationSetup(ConfigurationSetup configSetup)
        {
            _session.SetObjectAsJson(SessionPrefix + nameof(KernelConfiguration), configSetup.KernelConfiguration);
            _session.SetObjectAsJson(SessionPrefix + nameof(SearchConfiguration), configSetup.SearchConfiguration);
            _session.SetObjectAsJson(SessionPrefix + nameof(PromptConfiguration), configSetup.PromptConfiguration);
            _session.SetObjectAsJson(SessionPrefix + nameof(GenerationConfiguration), configSetup.GenerationConfiguration);
            _session.SetObjectAsJson(SessionPrefix + nameof(DemoAppConfigSetup), configSetup.DemoAppConfigSetup);
        }

        /// <summary>
        /// Generates a descriptive string indicating the source of the key's loaded state and the associated provider.
        /// </summary>
        /// <param name="loadedFromInput">A nullable boolean indicating the source of the key's loaded state.  <see langword="true"/> if the key was
        /// loaded from input, <see langword="false"/> if loaded from configuration,  or <see langword="null"/> if the
        /// key is not loaded.</param>
        /// <param name="provider">The name of the provider associated with the key's loaded state.  If <see langword="null"/>, "No Provider
        /// was chosen" will be used in the output.</param>
        /// <returns>A string describing the key's loaded state and the associated provider.  For example: "This key is currently
        /// loaded from input for provider ProviderName."</returns>
        private string GetLoadedFromString(bool? loadedFromInput, string? provider)
        {
            var loadedText = loadedFromInput is null ? "not loaded" : (((bool)loadedFromInput ? "loaded from input" : "loaded from configuration") + $" for provider {provider ?? "No Provider was chosen"}");
            return $"This key is currently {loadedText}.";
        }
    }
}
