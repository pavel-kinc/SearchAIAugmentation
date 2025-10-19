using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.SemanticKernel;
using PromptEnhancer.ChunkUtilities;
using PromptEnhancer.ChunkUtilities.Interfaces;
using PromptEnhancer.KernelServiceTemplates;
using PromptEnhancer.Search;
using PromptEnhancer.Search.Interfaces;
using PromptEnhancer.Services.EnhancerService;
using PromptEnhancer.SK;
using PromptEnhancer.SK.Interfaces;

namespace PromptEnhancer.Extensions
{
    public static class PromptEnhancerServiceCollectionExtensions
    {
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

        private static IServiceCollection AddInternalServices(this IServiceCollection services)
        {
            services.AddInMemoryVectorStore();
            services.TryAddSingleton<IKernelServiceFactory, KernelServiceFactory>();
            services.TryAddSingleton<IChunkGenerator, SemanticSlicerChunkGenerator>();
            services.TryAddSingleton<IChunkRanker, MiniLmL6V2ChunkRanker>();
            services.TryAddSingleton<ISearchProviderManager, SearchProviderManager>();
            services.TryAddSingleton<ISearchWebScraper, SearchWebScraper>();
            services.TryAddSingleton<ISemanticKernelManager, SemanticKernelManager>();

            return services;
        }

        private static void AddKernelToDI(this IServiceCollection services)
        {
            services.AddKernel();
        }
    }
}
