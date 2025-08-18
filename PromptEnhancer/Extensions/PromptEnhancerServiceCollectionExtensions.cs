using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
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
            services.Add(descriptor);
            services.AddInternalServices();
            return services;
        }

        private static IServiceCollection AddInternalServices(this IServiceCollection services)
        {
            services.TryAddSingleton<IChunkGenerator, SemanticSlicerChunkGenerator>();
            services.AddSingleton<IChunkRanker, MiniLmL6V2ChunkRanker>();
            services.AddSingleton<ISearchProviderManager, SearchProviderManager>();
            services.AddSingleton<ISearchWebScraper, SearchWebScraper>();
            services.AddSingleton<ISemanticKernelManager, SemanticKernelManager>();
            return services;
        }
    }
}
