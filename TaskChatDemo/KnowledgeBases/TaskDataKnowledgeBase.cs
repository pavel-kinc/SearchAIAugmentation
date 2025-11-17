using Microsoft.Extensions.AI;
using PromptEnhancer.KnowledgeBaseCore;
using PromptEnhancer.KnowledgeBaseCore.Interfaces;
using PromptEnhancer.KnowledgeRecord;
using PromptEnhancer.KnowledgeRecord.Interfaces;
using PromptEnhancer.KnowledgeSearchRequest.Interfaces;
using TaskChatDemo.Models.SearchFilterModels;
using TaskChatDemo.Models.Settings;
using TaskChatDemo.Models.TaskItem;
using TaskChatDemo.Services.VectorStore;

namespace TaskChatDemo.KnowledgeBases
{
    public class TaskDataKnowledgeBase : KnowledgeBase<KnowledgeRecord<TaskItemData>, SearchItemFilterModel, ItemDataSearchSettings, EmptyModelFilter<TaskItemData>, TaskItemData>
    {
        public IVectorStoreService _vectorStoreService;
        public TaskDataKnowledgeBase(IVectorStoreService vectorStoreService)
        {
            _vectorStoreService = vectorStoreService;
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
