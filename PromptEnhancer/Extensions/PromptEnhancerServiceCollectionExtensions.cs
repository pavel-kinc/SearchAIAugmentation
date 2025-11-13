using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.SemanticKernel;
using PromptEnhancer.ChunkUtilities;
using PromptEnhancer.ChunkUtilities.Interfaces;
using PromptEnhancer.KernelServiceTemplates;
using PromptEnhancer.KnowledgeBase;
using PromptEnhancer.KnowledgeBase.Examples;
using PromptEnhancer.KnowledgeBase.Interfaces;
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
using PromptEnhancer.Services.RankerService;
using PromptEnhancer.Services.RecordPickerService;
using PromptEnhancer.Services.RecordRankerService;
using PromptEnhancer.SK;
using PromptEnhancer.SK.Interfaces;

namespace PromptEnhancer.Extensions
{
    public static class PromptEnhancerServiceCollectionExtensions
    {
        // needs to be atleast scoped based on current logic
        public static IServiceCollection AddPromptEnhancer(this IServiceCollection services, ServiceLifetime lifetime = ServiceLifetime.Singleton, IEnumerable<IKernelServiceTemplate>? kernelServices = null, bool addKernelToDI = false)
        {
            var descriptor = new ServiceDescriptor(typeof(IEnhancerService), typeof(EnhancerService), lifetime);

            services.TryAdd(descriptor);
            services.AddInternalServices();

            if (kernelServices is not null && kernelServices.Any())
            {
                services.AddKernelServices(kernelServices);
            }

            if (addKernelToDI && !services.Any(sd => sd.ServiceType == typeof(Kernel)))
            {
                services.AddKernelToDI();
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

        public static void AddInternalServices(this IServiceCollection services)
        {
            services.AddInMemoryVectorStore();
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

            services.AddSingleton<ISemanticKernelContextPlugin, DateTimePlugin>();
            services.AddSingleton<ISemanticKernelContextPlugin, TemperaturePlugin>();


            //services.TryAddKeyedSingleton<IKnowledgeBase, TestKnowledgeBaseProcessor>("test");
            services.TryAddSingleton<IKnowledgeBase<KnowledgeUrlRecord, GoogleSearchFilterModel, GoogleSettings, UrlRecordFilter, UrlRecord>, GoogleKnowledgeBase>();
            //Delete prolly?
            services.TryAddSingleton<IPipelineContextService, PipelineContextService>();
        }

        private static void AddKernelToDI(this IServiceCollection services)
        {
            services.AddKernel();
        }
    }
}
