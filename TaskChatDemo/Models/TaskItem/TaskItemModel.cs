using AngleSharp.Browser;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.VectorData;

namespace TaskChatDemo.Models.TaskItem
{
    public class TaskItemModel
    {
        [VectorStoreKey]
        public Guid Id { get; set; } = Guid.NewGuid();
        [VectorStoreData]
        public required string Title { get; set; }
        [VectorStoreData]
        public string? Description { get; set; }
        [VectorStoreData]
        public string Status { get; set; } = "TODO";
        [VectorStoreData]
        public string? Assignee { get; set; }
        [VectorStoreData]
        public DateTime? CreatedDate { get; set; }
        [VectorStoreVector(Dimensions: 1536)]
        public ReadOnlyMemory<float>? Embedding { get; set; }
    }
}
