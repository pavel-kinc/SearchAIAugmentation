using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel;
using PromptEnhancer.KnowledgeRecord.Interfaces;
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

        public async Task<bool> GetSimilarityScoreForRecordsAsync(Kernel kernel, IEnumerable<IKnowledgeRecord> records,string? queryString, string? generatorKey = null)
        {
            var dict = new Dictionary<string, ReadOnlyMemory<float>>();
            var generator = kernel.GetRequiredService<IEmbeddingGenerator<string, Embedding<float>>>(generatorKey);
            if (queryString is not null)
            {
                var query = await generator.GenerateVectorAsync(queryString);
                dict.Add(queryString, query);
            }

            foreach (var record in records.Where(x => x.SimilarityScore is null))
            {
                //TODO what if 1 assignment fails? now i just ignore the result
                await TryAssignScoreToRecord(record, generator, dict);
            }
            return true;
        }

        private async Task<bool> TryAssignScoreToRecord(IKnowledgeRecord record, IEmbeddingGenerator<string, Embedding<float>> generator, Dictionary<string, ReadOnlyMemory<float>> dict, ReadOnlyMemory<float>? embed = null)
        {
            //TODO maybe just give it the basic query from context? but that could lead to some random data
            if (record.UsedSearchQuery is null)
            {
                return false;
            }

            if (!dict.ContainsKey(record.UsedSearchQuery))
            {
                dict.Add(record.UsedSearchQuery, await generator.GenerateVectorAsync(record.UsedSearchQuery));
            }

            var queryEmbed = dict[record.UsedSearchQuery];
            var recordEmbed = embed ?? record.Embeddings?.EmbeddingVector;

            if (recordEmbed is null)
            {
                return false;
            }

            record.SimilarityScore = _rankerService.GetSimilarityScore(queryEmbed, (ReadOnlyMemory<float>)recordEmbed);
            return true;
        }
    }
}
