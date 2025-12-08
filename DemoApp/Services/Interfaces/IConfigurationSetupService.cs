using DemoApp.Models;
using PromptEnhancer.Models.Configurations;

namespace DemoApp.Services.Interfaces
{
    /// <summary>
    /// Provides methods for managing and retrieving application configuration settings.
    /// </summary>
    /// <remarks>This interface defines operations for retrieving, updating, and clearing various
    /// configuration settings, including kernel, search, prompt, and generation configurations. It also supports
    /// uploading a complete configuration setup and optionally retrieving sensitive configuration details.</remarks>
    public interface IConfigurationSetupService
    {
        /// <summary>
        /// Retrieves the current configuration settings for the application.
        /// </summary>
        /// <remarks>Use caution when setting <paramref name="withSecrets"/> to <see langword="true"/> to
        /// avoid  exposing sensitive data. This method is typically used to retrieve configuration settings  for
        /// diagnostics, logging, or initialization purposes.</remarks>
        /// <param name="withSecrets">A value indicating whether sensitive information, such as secrets or credentials,  should be included in the
        /// configuration. Specify <see langword="true"/> to include secrets;  otherwise, <see langword="false"/>.</param>
        /// <returns>A <see cref="ConfigurationSetup"/> object containing the application's configuration settings.  If <paramref
        /// name="withSecrets"/> is <see langword="true"/>, the returned object includes  sensitive information;
        /// otherwise, sensitive information is omitted.</returns>
        ConfigurationSetup GetConfiguration(bool withSecrets = false);

        // Methods to work with singular configs and upload (they work with session)
        void UpdateKernelConfig(KernelConfiguration kernelConfiguration);
        void UpdateSearchConfig(SearchConfiguration searchConfiguration);
        void UpdatePromptConfig(PromptConfiguration promptConfiguration);
        void UpdateGenerationConfig(GenerationConfiguration generationConfiguration);
        void UploadConfiguration(ConfigurationSetup configuration);

        /// <summary>
        /// Clear current session for user
        /// </summary>
        void ClearSession();

    }
}
