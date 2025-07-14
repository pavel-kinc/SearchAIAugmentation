using DemoApp.Models;
using DemoApp.Services.Interfaces;
using Mapster;

using PromptEnhancer.ConfigurationHelper;
using PromptEnhancer.Models.Configurations;
using PromptEnhancer.Services;

namespace DemoApp.Services
{
    public class ConfigurationSetupService : IConfigurationSetupService
    {
        private ConfigurationSetup _enhancerConfig;

        private readonly IEnhancerService _enhancerService;

        private readonly IConfiguration _configuration;

        public ConfigurationSetupService(IConfiguration configuration, IEnhancerService enhancerService)
        {
            _configuration = configuration;
            _enhancerService = enhancerService;
            _enhancerConfig = GetDefaultConfiguration();
        }

        public ConfigurationSetup GetConfiguration(bool withSecrets = false)
        {
            var config = _enhancerConfig.Adapt<ConfigurationSetup>();
            if (!withSecrets)
            {
                SensitiveDataResolver.HideSensitiveProperties(config);
            }
            return config;
        }

        public void UpdateKernelConfig(KernelConfiguration kernelConfiguration)
        {
            if (kernelConfiguration.AIApiKey is not null)
            {
                _enhancerConfig.DemoAppConfigSetup.AIApiKeyFromInput = GetLoadedFromString(true, _enhancerConfig.KernelConfiguration.Provider.ToString());
            }
            kernelConfiguration.AIApiKey ??= _enhancerConfig.KernelConfiguration.AIApiKey;
            _enhancerConfig.KernelConfiguration = kernelConfiguration;
        }

        public void UpdateSearchConfig(SearchConfiguration searchConfiguration)
        {
            if (searchConfiguration.SearchProviderData.SearchApiKey is not null)
            {
                _enhancerConfig.DemoAppConfigSetup.SearchApiKeyFromInput = GetLoadedFromString(true, _enhancerConfig.SearchConfiguration.SearchProviderData.Provider.ToString());
            }
            if (searchConfiguration.SearchProviderData.Engine is not null)
            {
                _enhancerConfig.DemoAppConfigSetup.SearchEngineFromInput = GetLoadedFromString(true, _enhancerConfig.SearchConfiguration.SearchProviderData.Provider.ToString());
            }
            searchConfiguration.SearchProviderData.SearchApiKey ??= _enhancerConfig.SearchConfiguration.SearchProviderData.SearchApiKey;
            searchConfiguration.SearchProviderData.Engine ??= _enhancerConfig.SearchConfiguration.SearchProviderData.Engine;
            _enhancerConfig.SearchConfiguration = searchConfiguration;
        }

        public void UpdateDemoAppConfig(DemoAppConfigSetup demoAppConfigSetup)
        {
            _enhancerConfig.DemoAppConfigSetup = demoAppConfigSetup;
        }

        public void UploadConfiguration(ConfigurationSetup configuration)
        {
            _enhancerConfig = configuration;
            SetDefaultDemoAppConfig(_enhancerConfig);
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

        private string GetLoadedFromString(bool? loadedFromInput, string? provider)
        {
            var loadedText = loadedFromInput is null ? "not loaded" : (((bool)loadedFromInput ? "loaded from input" : "loaded from configuration") + $" for provider {provider ?? "No Provider was chosen"}");
            return $"This key is currently {loadedText}.";
        }

        public void UpdatePromptConfig(PromptConfiguration promptConfiguration)
        {
            _enhancerConfig.PromptConfiguration = promptConfiguration;
        }
    }
}
