using Microsoft.Extensions.DependencyInjection;
using PromptEnhancer.ChunkUtilities;
using PromptEnhancer.ChunkUtilities.Interfaces;
using PromptEnhancer.Search;
using PromptEnhancer.Search.Interfaces;
using PromptEnhancer.Services;
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
            services.AddTransient<IChunkGenerator, SemanticSlicerChunkGenerator>();
            services.AddSingleton<IChunkRanker, MiniLmL6V2ChunkRanker>();
            services.AddTransient<ISearchProviderManager, SearchProviderManager>();
            services.AddTransient<ISearchWebScraper, SearchWebScraper>();
            services.AddTransient<ISemanticKernelManager, SemanticKernelManager>();
            return services;
        }
    }
}
