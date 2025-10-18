using DemoApp.Models;
using DemoApp.Services.Interfaces;
using DemoApp.SessionUtility;
using Mapster;
using Newtonsoft.Json;
using PromptEnhancer.CustomJsonResolver;
using PromptEnhancer.Models.Configurations;
using PromptEnhancer.Services.EnhancerService;

namespace DemoApp.Services
{
    public class ConfigurationSetupService : IConfigurationSetupService
    {
        private readonly IEnhancerService _enhancerService;

        private readonly IConfiguration _configuration;

        private readonly ISession _session;

        private const string SessionPrefix = "Config.";

        public ConfigurationSetupService(IConfiguration configuration, IEnhancerService enhancerService, IHttpContextAccessor ctx)
        {


            _configuration = configuration;
            _enhancerService = enhancerService;
            _session = ctx.HttpContext!.Session;
            if (!_session.Keys.Any(k => k.StartsWith(SessionPrefix)))
            {
                var config = GetDefaultConfiguration();
                SetConfigurationSetup(config);
            }
        }

        public ConfigurationSetup GetConfiguration(bool withSecrets = false)
        {
            var config = new ConfigurationSetup
            {
                KernelConfiguration = _session.GetObjectFromJson<KernelConfiguration>(SessionPrefix + nameof(KernelConfiguration))!,
                SearchConfiguration = _session.GetObjectFromJson<SearchConfiguration>(SessionPrefix + nameof(SearchConfiguration))!,
                PromptConfiguration = _session.GetObjectFromJson<PromptConfiguration>(SessionPrefix + nameof(PromptConfiguration))!,
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

            demoAppConfig!.SearchApiKeyFromInput = searchConfiguration.SearchProviderData.SearchApiKey is not null ? GetLoadedFromString(true, searchConfig?.SearchProviderData.Provider.ToString()) : demoAppConfig.SearchApiKeyFromInput;
            demoAppConfig!.SearchEngineFromInput = searchConfiguration.SearchProviderData.Engine is not null ? GetLoadedFromString(true, searchConfig?.SearchProviderData.Provider.ToString()) : demoAppConfig.SearchEngineFromInput;

            searchConfiguration.SearchProviderData.SearchApiKey ??= searchConfig!.SearchProviderData.SearchApiKey;
            searchConfiguration.SearchProviderData.Engine ??= searchConfig!.SearchProviderData.Engine;
            SetSessionPartialConfigAndDemoAppConfig(nameof(SearchConfiguration), searchConfiguration, demoAppConfig);
        }

        public void UpdatePromptConfig(PromptConfiguration promptConfiguration)
        {
            var promptConfig = _session.GetObjectFromJson<PromptConfiguration>(SessionPrefix + nameof(PromptConfiguration));
            _session.SetObjectAsJson(SessionPrefix + nameof(PromptConfiguration), promptConfiguration);
        }

        public void UpdateDemoAppConfig(DemoAppConfigSetup demoAppConfigSetup)
        {
            var demoAppConfig = _session.GetObjectFromJson<DemoAppConfigSetup>(SessionPrefix + nameof(DemoAppConfigSetup));
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

        public void SetSessionPartialConfigAndDemoAppConfig(string configName, object partialConfig, object? demoAppConfig)
        {
            _session.SetObjectAsJson(SessionPrefix + configName, partialConfig);
            if (demoAppConfig is not null)
            {
                _session.SetObjectAsJson(SessionPrefix + nameof(DemoAppConfigSetup), demoAppConfig);
            }
        }

        private ConfigurationSetup GetDefaultConfiguration()
        {
            var enhancerConfig = _enhancerService.CreateDefaultConfiguration(aiApiKey: _configuration["AIServices:OpenAI:ApiKey"], searchApiKey: _configuration["SearchConfigurations:Google:ApiKey"], searchEngine: _configuration["SearchConfigurations:Google:SearchEngineId"]);
            var configSetup = enhancerConfig.Adapt<ConfigurationSetup>();

            SetDefaultDemoAppConfig(configSetup);
            return configSetup;
        }

        private void SetDefaultDemoAppConfig(ConfigurationSetup configSetup)
        {
            var searchProvider = configSetup.SearchConfiguration.SearchProviderData.Provider.ToString();
            configSetup.DemoAppConfigSetup.AIApiKeyFromInput = GetLoadedFromString(configSetup.KernelConfiguration.AIApiKey is not null ? false : null, configSetup.KernelConfiguration.Provider.ToString());
            configSetup.DemoAppConfigSetup.SearchApiKeyFromInput = GetLoadedFromString(configSetup.SearchConfiguration.SearchProviderData.SearchApiKey is not null ? false : null, searchProvider);
            configSetup.DemoAppConfigSetup.SearchEngineFromInput = GetLoadedFromString(configSetup.SearchConfiguration.SearchProviderData.Engine is not null ? false : null, searchProvider);
        }

        private void SetConfigurationSetup(ConfigurationSetup configSetup)
        {
            _session.SetObjectAsJson(SessionPrefix + nameof(KernelConfiguration), configSetup.KernelConfiguration);
            _session.SetObjectAsJson(SessionPrefix + nameof(SearchConfiguration), configSetup.SearchConfiguration);
            _session.SetObjectAsJson(SessionPrefix + nameof(PromptConfiguration), configSetup.PromptConfiguration);
            _session.SetObjectAsJson(SessionPrefix + nameof(DemoAppConfigSetup), configSetup.DemoAppConfigSetup);
        }

        private string GetLoadedFromString(bool? loadedFromInput, string? provider)
        {
            var loadedText = loadedFromInput is null ? "not loaded" : (((bool)loadedFromInput ? "loaded from input" : "loaded from configuration") + $" for provider {provider ?? "No Provider was chosen"}");
            return $"This key is currently {loadedText}.";
        }
    }
}
