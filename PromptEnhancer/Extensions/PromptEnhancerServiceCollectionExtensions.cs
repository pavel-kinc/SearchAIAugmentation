using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.SemanticKernel;
using PromptEnhancer.ChunkUtilities;
using PromptEnhancer.ChunkUtilities.Interfaces;
using PromptEnhancer.KernelServiceTemplates;
using PromptEnhancer.KnowledgeBaseCore;
using PromptEnhancer.KnowledgeBaseCore.Examples;
using PromptEnhancer.KnowledgeBaseCore.Interfaces;
using PromptEnhancer.KnowledgeRecord;
using PromptEnhancer.KnowledgeSearchRequest.Examples;
using PromptEnhancer.Models.Examples;
using PromptEnhancer.Models.Pipeline;
using PromptEnhancer.Pipeline;
using PromptEnhancer.Pipeline.Interfaces;
using PromptEnhancer.Plugins;
using PromptEnhancer.Plugins.Interfaces;
using PromptEnhancer.Search;
using PromptEnhancer.Search.Interfaces;
using PromptEnhancer.Services.EmbeddingService;
using PromptEnhancer.Services.EnhancerService;
using PromptEnhancer.Services.PromptBuildingService;
using PromptEnhancer.Services.RankerService;
using PromptEnhancer.Services.RecordPickerService;
using PromptEnhancer.Services.RecordRankerService;
using PromptEnhancer.SK;
using PromptEnhancer.SK.Interfaces;
using System.Runtime.CompilerServices;

namespace PromptEnhancer.Extensions
{
    public static class PromptEnhancerServiceCollectionExtensions
    {
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

            if (addKernelToDI && !services.Any(sd => sd.ServiceType == typeof(Kernel)))
            {
                services.AddKernelToDI(kernelServices);
            }

            return services;
        }

        public static IServiceCollection AddKernelServices(this IServiceCollection services, IEnumerable<IKernelServiceTemplate> kernelServices)
        {
            foreach (var template in kernelServices)
            {
                template.AddToServices(services);
            }

            return services;
        }

        public static void AddSemanticKernelContextPlugins(this IServiceCollection services)
        {
            services.AddSingleton<ISemanticKernelContextPlugin, DateTimePlugin>();
            services.AddSingleton<ISemanticKernelContextPlugin, TemperaturePlugin>();
        }

        public static void AddGoogleKnowledgeBase(this IServiceCollection services)
        {
            services.TryAddSingleton<IKnowledgeBase<KnowledgeUrlRecord, GoogleSearchFilterModel, GoogleSettings, UrlRecordFilter, UrlRecord>, GoogleKnowledgeBase>();
            services.TryAddSingleton<GoogleKnowledgeBase, GoogleKnowledgeBase>();
        }

        public static void AddInternalServices(this IServiceCollection services)
        {
            services.TryAddSingleton<IKernelServiceFactory, KernelServiceFactory>();
            services.TryAddSingleton<IChunkGeneratorService, SemanticSlicerChunkService>();
            services.TryAddSingleton<IChunkRankerService, MiniLmL6V2ChunkRanker>();
            services.TryAddSingleton<ISearchProviderManager, SearchProviderManager>();
            services.TryAddSingleton<ISearchWebScraper, SearchWebScraper>();
            services.TryAddSingleton<ISemanticKernelManager, SemanticKernelManager>();
            services.TryAddSingleton<IPipelineOrchestrator, PipelineOrchestrator>();
            services.TryAddSingleton<IEmbeddingService, EmbeddingService>();
            services.TryAddSingleton<IRecordRankerService, RecordRankerService>();
            services.TryAddSingleton<IRankerService, CosineSimilarityRankerService>();
            services.TryAddSingleton<IRecordPickerService, RecordPickerService>();
            services.TryAddSingleton<IPromptBuildingService, PromptBuildingService>();
        }

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
