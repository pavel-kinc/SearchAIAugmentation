using Microsoft.Extensions.AI;
using PromptEnhancer.KnowledgeBaseCore;
using PromptEnhancer.KnowledgeRecord;
using PromptEnhancer.KnowledgeRecord.Interfaces;
using PromptEnhancer.KnowledgeSearchRequest.Interfaces;
using TaskChatDemo.Models.SearchFilterModels;
using TaskChatDemo.Models.Settings;
using TaskChatDemo.Models.TaskItem;
using TaskChatDemo.Services.VectorStore;

namespace TaskChatDemo.KnowledgeBases
{
    /// <summary>
    /// Represents a specialized knowledge base for managing and searching development task data.
    /// </summary>
    /// <remarks>This knowledge base is designed to handle information about development tasks, such as who
    /// performed specific tasks.  It supports searching for relevant task data using vector-based similarity matching.
    /// Queries that are not explicitly  tied to other knowledge bases are likely referencing this knowledge
    /// base.</remarks>
    public class TaskDataKnowledgeBase : KnowledgeBase<KnowledgeRecord<TaskItemData>, SearchItemFilterModel, ItemDataSearchSettings, EmptyModelFilter<TaskItemData>, TaskItemData>
    {
        public IVectorStoreService _vectorStoreService;
        private readonly ILogger<TaskDataKnowledgeBase> _logger;

        public TaskDataKnowledgeBase(IVectorStoreService vectorStoreService, ILogger<TaskDataKnowledgeBase> logger)
        {
            _vectorStoreService = vectorStoreService;
            _logger = logger;
        }
        public override string Description => "This knowledge base uses TaskData, that contain information about development tasks (who did what). Any standalone (out of place) part of the query is probably referencing this base.";
        public async override Task<IEnumerable<KnowledgeRecord<TaskItemData>>> SearchAsync(IKnowledgeSearchRequest<SearchItemFilterModel, ItemDataSearchSettings> request, IEnumerable<string> queriesToSearch, EmptyModelFilter<TaskItemData>? filter = null, CancellationToken ct = default)
        {
            var query = queriesToSearch.FirstOrDefault();
            if (string.IsNullOrWhiteSpace(query))
            {
                return [];
            }
            var queryVector = await request.Settings.Generator.GenerateVectorAsync(query, cancellationToken: ct);
            var data = await _vectorStoreService.GetDataWithScoresAsync(queryVector, request.Filter);
            var res = GetKnowledgeRecords(data, filter, query, false, ct: ct);
            _logger.LogInformation("TaskDataKnowledgeBase returned {Count} records for query: {Query}", res.Count(), query);
            return res;
        }
        protected override KnowledgeRecord<TaskItemData> CreateRecord(TaskItemData o, string queryString)
        {
            return new KnowledgeRecord<TaskItemData>
            {
                Id = Guid.NewGuid().ToString(),
                SourceObject = o,
                Source = GetType().Name,
                UsedSearchQuery = queryString,
                //adding similarity score here
                SimilarityScore = o.SimilarityScore,
            };
        }
    }
}
