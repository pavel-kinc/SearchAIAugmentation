using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using PromptEnhancer.KnowledgeRecord.Interfaces;
using PromptEnhancer.Services.RankerService;

namespace PromptEnhancer.Services.RecordRankerService
{
    /// <summary>
    /// Provides functionality to assign similarity scores to knowledge records based on embeddings and a query string.
    /// </summary>
    /// <remarks>This service uses an embedding generator and a ranker service to calculate similarity scores
    /// for knowledge records. It supports assigning scores based on a provided query string and optionally allows
    /// specifying a generator key to resolve the appropriate embedding generator. Records with existing embeddings or
    /// null similarity scores are recalculated.</remarks>
    public class RecordRankerService : IRecordRankerService
    {
        private readonly IRankerService _rankerService;
        private readonly ILogger<RecordRankerService> _logger;

        public RecordRankerService(IRankerService rankerService, ILogger<RecordRankerService> logger)
        {
            _rankerService = rankerService;
            _logger = logger;
        }

        // TODO add smth to embed together with queryString - for example if user searches only ean, it is compared to only ean embeddings (number)
        /// <inheritdoc/>
        public async Task<bool> AssignSimilarityScoreToRecordsAsync(Kernel kernel, IEnumerable<IKnowledgeRecord> records, string? queryString, string? generatorKey = null)
        {
            _logger.LogInformation("Assigning similarity scores to {RecordCount} records using query: {QueryString}", records.Count(), queryString ?? "null");
            var dict = new Dictionary<string, Embedding<float>>();
            var generator = kernel.GetRequiredService<IEmbeddingGenerator<string, Embedding<float>>>(generatorKey);
            _logger.LogInformation("Using embedding generator with key: {GeneratorKey}", generatorKey ?? "default");
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

        /// <summary>
        /// Attempts to assign a similarity score to the specified knowledge record based on the provided embeddings.
        /// </summary>
        /// <remarks>This method calculates the similarity score between the embedding of the record's
        /// search query and the embedding of the record itself. If the record does not have a valid search query or
        /// embeddings, or if the similarity score cannot be computed, the method returns <see langword="false"/>. The
        /// method uses the provided dictionary to cache embeddings for search queries, reducing redundant
        /// computations.</remarks>
        /// <param name="record">The knowledge record to which the similarity score will be assigned. Must not be null.</param>
        /// <param name="generator">The embedding generator used to create embeddings for the search query if not already cached. Must not be
        /// null.</param>
        /// <param name="dict">A dictionary that caches embeddings for search queries. The method will add new entries if necessary. Must
        /// not be null.</param>
        /// <param name="embed">An optional embedding to use for the record. If null, the method will attempt to use the record's existing
        /// embeddings.</param>
        /// <returns><see langword="true"/> if a similarity score was successfully assigned to the record; otherwise, <see
        /// langword="false"/>.</returns>
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
                _logger.LogWarning("RecordRankerService could not assign score to record with ID {RecordId} due to missing embeddings. Source: {Source}", record.Id ?? "null", record.Source ?? "unknown");
                return false;
            }

            var score = _rankerService.GetSimilarityScore(queryEmbed, recordEmbed);

            if (score is null)
            {
                _logger.LogWarning("RecordRankerService could not compute similarity score for record with ID {RecordId}. Source: {Source}", record.Id ?? "null", record.Source ?? "unknown");
                return false;
            }

            record.SimilarityScore = score;
            return true;
        }
    }
}
