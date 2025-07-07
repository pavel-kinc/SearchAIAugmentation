using Microsoft.Extensions.DependencyInjection;

using PromptEnhancer.Services;

namespace PromptEnhancer.Extensions
{
    public static class PromptEnhancerServiceCollectionExtensions
    {
        public static IServiceCollection AddPromptEnhancer(this IServiceCollection services, ServiceLifetime lifetime = ServiceLifetime.Singleton)
        {
            var descriptor = new ServiceDescriptor(typeof(IEnhancerService), typeof(EnhancerService), lifetime);
            services.Add(descriptor);
            return services;
        }
    }
}
