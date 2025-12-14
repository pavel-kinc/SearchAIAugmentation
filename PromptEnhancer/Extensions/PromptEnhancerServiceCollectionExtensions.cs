using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.SemanticKernel;
using PromptEnhancer.KernelServiceTemplates;
using PromptEnhancer.KnowledgeBaseCore;
using PromptEnhancer.KnowledgeBaseCore.Examples;
using PromptEnhancer.KnowledgeBaseCore.Interfaces;
using PromptEnhancer.KnowledgeRecord;
using PromptEnhancer.KnowledgeSearchRequest.Examples;
using PromptEnhancer.Models.Examples;
using PromptEnhancer.Pipeline;
using PromptEnhancer.Pipeline.Interfaces;
using PromptEnhancer.Plugins;
using PromptEnhancer.Plugins.Interfaces;
using PromptEnhancer.Search;
using PromptEnhancer.Search.Interfaces;
using PromptEnhancer.Services.ChatHistoryService;
using PromptEnhancer.Services.ChunkService;
using PromptEnhancer.Services.EmbeddingService;
using PromptEnhancer.Services.EnhancerService;
using PromptEnhancer.Services.PromptBuildingService;
using PromptEnhancer.Services.RankerService;
using PromptEnhancer.Services.RecordPickerService;
using PromptEnhancer.Services.RecordRankerService;
using PromptEnhancer.Services.TokenCounterService;
using PromptEnhancer.SK;
using PromptEnhancer.SK.Interfaces;

namespace PromptEnhancer.Extensions
{
    /// <summary>
    /// Provides extension methods for adding prompt enhancer services to an <see cref="IServiceCollection"/>.
    /// </summary>
    /// <remarks>This class includes methods to register various services related to prompt enhancement,
    /// including context plugins, Google knowledge base integration, and kernel services. These methods facilitate the
    /// configuration and setup of services required for prompt enhancement in a dependency injection
    /// container.</remarks>
    public static class PromptEnhancerServiceCollectionExtensions
    {
        /// <summary>
        /// Adds the prompt enhancer services, mainly the IEnhancerService and IKernelManager, to the specified <see cref="IServiceCollection"/>.
        /// You can inject these services into your classes by requesting an <see cref="IEnhancerService"/> or <see cref="ISemanticKernelManager"/>
        /// </summary>
        /// <remarks>This method configures the service collection with the necessary services for
        /// enhancing prompts, including optional context plugins and a Google Knowledge Base. It also allows for the
        /// addition of kernel services and the kernel itself to the dependency injection container.</remarks>
        /// <param name="services">The <see cref="IServiceCollection"/> to which the services are added.</param>
        /// <param name="lifetime">The <see cref="ServiceLifetime"/> of the enhancer service. Defaults to <see
        /// cref="ServiceLifetime.Singleton"/>.</param>
        /// <param name="addContextPlugins">If <see langword="true"/>, adds context plugins to the service collection. Defaults to <see
        /// langword="true"/>.</param>
        /// <param name="addGoogleKnowledgeBase">If <see langword="true"/>, adds the Google Knowledge Base to the service collection. Defaults to <see
        /// langword="true"/>.</param>
        /// <param name="addKernelToDI">If <see langword="true"/>, adds the kernel to the dependency injection container. Defaults to <see
        /// langword="false"/>.</param>
        /// <param name="kernelServices">An optional collection of kernel services to add. If <see langword="null"/> or empty, no kernel services are
        /// added.</param>
        /// <returns>The modified <see cref="IServiceCollection"/>.</returns>
        public static IServiceCollection AddPromptEnhancer(this IServiceCollection services, ServiceLifetime lifetime = ServiceLifetime.Singleton, bool addContextPlugins = true, bool addGoogleKnowledgeBase = true, bool addKernelToDI = false, IEnumerable<IKernelServiceTemplate>? kernelServices = null)
        {
            var descriptor = new ServiceDescriptor(typeof(IEnhancerService), typeof(EnhancerService), lifetime);

            services.TryAdd(descriptor);
            services.AddInternalServices();

            if (addContextPlugins)
            {
                services.AddSemanticKernelContextPlugins();
            }

            if (addGoogleKnowledgeBase)
            {
                services.AddGoogleKnowledgeBase();
            }

            if (kernelServices is not null && kernelServices.Any())
            {
                services.AddKernelServices(kernelServices);
            }

            // add only if not already added
            if (addKernelToDI && !services.Any(sd => sd.ServiceType == typeof(Kernel)))
            {
                services.AddKernelToDI(kernelServices);
            }

            return services;
        }

        /// <summary>
        /// Adds kernel services to the specified service collection.
        /// </summary>
        /// <param name="services">The service collection to which the kernel services will be added.</param>
        /// <param name="kernelServices">A collection of kernel service templates to be added to the service collection.</param>
        /// <returns>The updated service collection with the kernel services added.</returns>
        public static IServiceCollection AddKernelServices(this IServiceCollection services, IEnumerable<IKernelServiceTemplate> kernelServices)
        {
            foreach (var template in kernelServices)
            {
                template.AddToServices(services);
            }

            return services;
        }

        /// <summary>
        /// Adds the Semantic Kernel context plugins to the service collection. It can be used to add context plugins into Kernel services.
        /// </summary>
        /// <remarks>This method registers the <see cref="ISemanticKernelContextPlugin"/> implementations
        /// with the provided <see cref="IServiceCollection"/>. It is typically used during application startup to
        /// configure dependency injection.</remarks>
        /// <param name="services">The service collection to which the plugins are added.</param>
        public static void AddSemanticKernelContextPlugins(this IServiceCollection services)
        {
            services.AddSingleton<ISemanticKernelContextPlugin, DateTimePlugin>();
        }

        /// <summary>
        /// Adds the Google Knowledge Base services to the specified service collection.
        /// </summary>
        /// <remarks>This method registers the <see cref="GoogleKnowledgeBase"/> and its related
        /// interfaces as singleton services. It is intended to be used during application startup to configure
        /// dependency injection.</remarks>
        /// <param name="services">The <see cref="IServiceCollection"/> to which the Google Knowledge Base services will be added.</param>
        public static void AddGoogleKnowledgeBase(this IServiceCollection services)
        {
            services.TryAddSingleton<IKnowledgeBase<KnowledgeUrlRecord, GoogleSearchFilterModel, GoogleSettings, UrlRecordFilter, UrlRecord>, GoogleKnowledgeBase>();
            services.TryAddSingleton<GoogleKnowledgeBase, GoogleKnowledgeBase>();
        }

        /// <summary>
        /// Adds internal services to the specified <see cref="IServiceCollection"/>.
        /// </summary>
        /// <remarks>This method registers a set of singleton services that are used internally within the
        /// application. It is intended to be used during the application's startup configuration to ensure that all
        /// necessary services are available for dependency injection.</remarks>
        /// <param name="services">The <see cref="IServiceCollection"/> to which the internal services are added.</param>
        public static void AddInternalServices(this IServiceCollection services)
        {
            services.TryAddSingleton<IKernelServiceFactory, KernelServiceFactory>();
            services.TryAddSingleton<IChunkGeneratorService, SemanticSlicerChunkService>();
            services.TryAddSingleton<ISearchProviderManager, SearchProviderManager>();
            services.TryAddSingleton<ISearchWebScraper, SearchWebScraper>();
            services.TryAddSingleton<ISemanticKernelManager, SemanticKernelManager>();
            services.TryAddSingleton<IPipelineOrchestrator, PipelineOrchestrator>();
            services.TryAddSingleton<IEmbeddingService, EmbeddingService>();
            services.TryAddSingleton<IRecordRankerService, RecordRankerService>();
            services.TryAddSingleton<IRankerService, CosineSimilarityRankerService>();
            services.TryAddSingleton<IRecordPickerService, RecordPickerService>();
            services.TryAddSingleton<IPromptBuildingService, PromptBuildingService>();
            services.TryAddSingleton<ITokenCounterService, TokenCounterService>();
            services.TryAddSingleton<IChatHistoryService, ChatHistoryService>();
        }

        /// <summary>
        /// Adds the kernel and optional kernel services to the dependency injection container.
        /// </summary>
        /// <remarks>This method extends the <see cref="IServiceCollection"/> to include the kernel setup.
        /// If <paramref name="kernelServices"/> is specified and contains elements, those services are added to the
        /// kernel configuration.</remarks>
        /// <param name="services">The service collection to which the kernel and services are added.</param>
        /// <param name="kernelServices">An optional collection of kernel services to add. If provided, these services will be registered with the
        /// kernel.</param>
        private static void AddKernelToDI(this IServiceCollection services, IEnumerable<IKernelServiceTemplate>? kernelServices = null)
        {
            var builder = services.AddKernel();
            if (kernelServices is not null && kernelServices.Any())
            {
                builder.Services.AddKernelServices(kernelServices);
            }
        }
    }
}
