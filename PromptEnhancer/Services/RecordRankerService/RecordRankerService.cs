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

        public async Task<bool> GetSimilarityScoreForRecordsAsync(Kernel kernel, IEnumerable<IKnowledgeRecord> records, string? queryString, string? generatorKey = null)
        {
            var dict = new Dictionary<string, Embedding<float>>();
            var generator = kernel.GetRequiredService<IEmbeddingGenerator<string, Embedding<float>>>(generatorKey);
            if (queryString is not null)
            {
                var query = await generator.GenerateAsync(queryString);
                dict.Add(queryString, query);
            }

            //TODO what if it needs rewrite for repetitive context? (now its like this for me to not rewrite sent data from base that returns them)
            // if there are embeddings, it just recalculates
            foreach (var record in records.Where(x => x.SimilarityScore is null || x.Embeddings is not null))
            {
                //TODO what if 1 assignment fails? now i just ignore the result
                await TryAssignScoreToRecord(record, generator, dict);
            }
            return true;
        }

        private async Task<bool> TryAssignScoreToRecord(IKnowledgeRecord record, IEmbeddingGenerator<string, Embedding<float>> generator, Dictionary<string, Embedding<float>> dict, Embedding<float>? embed = null)
        {
            //TODO maybe just give it the basic query from context? but that could lead to some random data
            if (record.UsedSearchQuery is null)
            {
                return false;
            }

            if (!dict.ContainsKey(record.UsedSearchQuery))
            {
                dict.Add(record.UsedSearchQuery, await generator.GenerateAsync(record.UsedSearchQuery));
            }

            var queryEmbed = dict[record.UsedSearchQuery];
            var recordEmbed = embed;
            if (recordEmbed is null && record.Embeddings is not null)
            {
                recordEmbed = new Embedding<float>(record.Embeddings.EmbeddingVector)
                {
                    ModelId = record.Embeddings.EmbeddingModel,
                };
            }

            if (recordEmbed is null)
            {
                return false;
            }

            var score = _rankerService.GetSimilarityScore(queryEmbed, recordEmbed);

            if (score is null)
            {
                return false;
            }

            record.SimilarityScore = score;
            return true;
        }
    }
}
