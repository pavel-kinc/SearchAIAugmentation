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
    /// <summary>
    /// Represents a knowledge base for managing and searching work items, which contain information about development
    /// work, such as task assignments and contributions.
    /// </summary>
    /// <remarks>This knowledge base is designed to handle queries related to work items. It provides
    /// functionality to search for work items based on specific filters and settings. Any standalone or out-of-context
    /// part of a query is likely referencing this knowledge base.</remarks>
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
