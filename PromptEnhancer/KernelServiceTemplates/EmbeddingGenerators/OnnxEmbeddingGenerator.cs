using Microsoft.Extensions.DependencyInjection;

namespace PromptEnhancer.KernelServiceTemplates.EmbeddingGenerators
{
    public class OnnxEmbeddingGenerator : KernelServiceTemplate
    {
        public string OnnxModelPath { get; }
        public string VocabPath { get; }
        public string? ServiceId { get; }

        public OnnxEmbeddingGenerator(string onnxModelPath, string vocabPath, string? serviceId = null)
        {
            OnnxModelPath = onnxModelPath;
            VocabPath = vocabPath;
            ServiceId = serviceId;
        }

        public override IServiceCollection AddToServices(IServiceCollection services)
        {
            services.AddBertOnnxEmbeddingGenerator(OnnxModelPath, VocabPath, serviceId: ServiceId);
            return services;
        }
    }
}
