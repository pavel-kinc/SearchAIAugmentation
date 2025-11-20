using PromptEnhancer.KnowledgeBaseCore;
using PromptEnhancer.KnowledgeRecord;
using PromptEnhancer.KnowledgeRecord.Interfaces;
using PromptEnhancer.KnowledgeSearchRequest.Interfaces;
using TaskChatDemo.Models;
using TaskChatDemo.Models.SearchFilterModels;
using TaskChatDemo.Models.Settings;
using TaskChatDemo.Services.ApiConsumer;

namespace TaskChatDemo.KnowledgeBases
{
    public class WorkItemKnowledgeBase : KnowledgeBase<KnowledgeRecord<WorkItem>, SearchWorkItemFilterModel, WorkItemSearchSettings, EmptyModelFilter<WorkItem>, WorkItem>
    {
        private readonly IWorkItemApiService _workItemApiService;
        public WorkItemKnowledgeBase(IWorkItemApiService workItemApiService)
        {
            _workItemApiService = workItemApiService;
        }

        public override string Description => "This knowledge base uses WorkItem, that contain information about development work (who did what). Any standalone (out of place) part of the query is probably referencing this base.";
        public async override Task<IEnumerable<KnowledgeRecord<WorkItem>>> SearchAsync(IKnowledgeSearchRequest<SearchWorkItemFilterModel, WorkItemSearchSettings> request, IEnumerable<string> queriesToSearch, EmptyModelFilter<WorkItem>? filter = null, CancellationToken ct = default)
        {
            var query = queriesToSearch.FirstOrDefault();
            if (string.IsNullOrWhiteSpace(query))
            {
                return [];
            }
            var workItems = await _workItemApiService.GetWorkItemsAsync(request.Filter, request.Settings.ApiUrl);
            var res = GetKnowledgeRecords(workItems, filter, query, false, ct: ct);
            return res;
        }
    }
}
