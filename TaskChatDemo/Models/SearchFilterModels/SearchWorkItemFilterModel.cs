using PromptEnhancer.KnowledgeBaseCore.Interfaces;

namespace TaskChatDemo.Models.SearchFilterModels
{
    public class SearchWorkItemFilterModel : IKnowledgeBaseSearchFilter
    {
        public string? Title { get; set; }
        public string? Type { get; set; }
        public string? Description { get; init; }
        public string? AssignedTo { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }

        public string BuildQuery()
        {
            var list = new List<string>();

            if (!string.IsNullOrWhiteSpace(Title))
                list.Add($"Title={Uri.EscapeDataString(Title)}");

            if (!string.IsNullOrWhiteSpace(Type))
                list.Add($"Type={Uri.EscapeDataString(Type)}");

            if (!string.IsNullOrWhiteSpace(Description))
                list.Add($"Description={Uri.EscapeDataString(Description)}");

            if (!string.IsNullOrWhiteSpace(AssignedTo))
                list.Add($"AssignedTo={Uri.EscapeDataString(AssignedTo)}");

            if (From.HasValue)
                list.Add($"From={Uri.EscapeDataString(From.Value.ToString("yyyy-MM-ddTHH:mm:ss"))}");

            if (To.HasValue)
                list.Add($"To={Uri.EscapeDataString(To.Value.ToString("yyyy-MM-ddTHH:mm:ss"))}");

            return string.Join("&", list);
        }
    }
}
