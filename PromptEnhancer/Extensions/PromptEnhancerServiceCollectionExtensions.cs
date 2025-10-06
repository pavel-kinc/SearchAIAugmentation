using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.SemanticKernel;
using PromptEnhancer.ChunkUtilities;
using PromptEnhancer.ChunkUtilities.Interfaces;
using PromptEnhancer.Search;
using PromptEnhancer.Search.Interfaces;
using PromptEnhancer.Services;
using PromptEnhancer.Services.Interfaces;
using PromptEnhancer.SK;
using PromptEnhancer.SK.Interfaces;

namespace PromptEnhancer.Extensions
{
    public static class PromptEnhancerServiceCollectionExtensions
    {
        public static IServiceCollection AddPromptEnhancer(this IServiceCollection services, ServiceLifetime lifetime = ServiceLifetime.Singleton)
        {
            var descriptor = new ServiceDescriptor(typeof(IEnhancerService), typeof(EnhancerService), lifetime);
            services.TryAdd(descriptor);
            services.AddInternalServices();
            return services;
        }

        private static IServiceCollection AddInternalServices(this IServiceCollection services)
        {
            services.AddInMemoryVectorStore();
            services.TryAddSingleton<IChunkGenerator, SemanticSlicerChunkGenerator>();
            services.TryAddSingleton<IChunkRanker, MiniLmL6V2ChunkRanker>();
            services.TryAddSingleton<ISearchProviderManager, SearchProviderManager>();
            services.TryAddSingleton<ISearchWebScraper, SearchWebScraper>();
            services.TryAddSingleton<ISemanticKernelManager, SemanticKernelManager>();
            return services;
        }
    }
}
