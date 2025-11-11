using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel;
using PromptEnhancer.KnowledgeRecord.Interfaces;
using PromptEnhancer.Models.Pipeline;
using PromptEnhancer.Services.RankerService;

namespace PromptEnhancer.Services.RecordRankerService
{
    public class RecordRankerService : IRecordRankerService
    {
        private readonly IRankerService _rankerService;

        // maybe resolve here by key?
        public RecordRankerService(IRankerService rankerService)
        {
            _rankerService = rankerService;
        }

        public async Task<IEnumerable<PipelineRankedRecord>> GetEmbeddingsForRecordsWithoutEmbeddingDataAsync(Kernel kernel, PipelineContext context, string? generatorKey = null)
        {
            var dict = new Dictionary<string, ReadOnlyMemory<float>>();
            var generator = kernel.GetRequiredService<IEmbeddingGenerator<string, Embedding<float>>>(generatorKey);
            if (context.QueryString is not null)
            {
                var query = await generator.GenerateVectorAsync(context.QueryString);
                dict.Add(context.QueryString, query);
            }
            ICollection<PipelineRankedRecord> res = [];

            foreach (var record in context.RetrievedRecords.Where(x => x.HasEmbeddingData))
            {
                if (record.SimilarityScore is not null)
                {
                    res.Add(new PipelineRankedRecord
                    {
                        AssociatedRecord = record,
                        SimilarityScore = record.SimilarityScore.Value
                    });
                }
                else
                {
                    var ranked = await CreateRankedRecord(record, generator, dict);
                    if (ranked is not null) res.Add(ranked);
                }
            }

            foreach (var embedRecord in context.PipelineEmbeddingsModels)
            {
                var ranked = await CreateRankedRecord(embedRecord.AssociatedRecord, generator, dict, embedRecord.EmbeddingVector);
                if (ranked is not null) res.Add(ranked);
            }
            return res;
        }

        private async Task<PipelineRankedRecord?> CreateRankedRecord(IKnowledgeRecord record, IEmbeddingGenerator<string, Embedding<float>> generator, Dictionary<string, ReadOnlyMemory<float>> dict, ReadOnlyMemory<float>? embed = null)
        {
            //TODO maybe just give it the basic query from context? but that could lead to some random data
            if (record.UsedSearchQuery is null)
            {
                return null;
            }

            if (!dict.ContainsKey(record.UsedSearchQuery))
            {
                dict.Add(record.UsedSearchQuery, await generator.GenerateVectorAsync(record.UsedSearchQuery));
            }

            var queryEmbed = dict[record.UsedSearchQuery];
            var recordEmbed = embed ?? record.GivenEmbeddings;

            if (recordEmbed is null)
            {
                return null;
            }

            var res = _rankerService.GetSimilarityScore(queryEmbed, (ReadOnlyMemory<float>)recordEmbed);
            return new PipelineRankedRecord
            {
                AssociatedRecord = record,
                SimilarityScore = res
            };
        }
    }
}
