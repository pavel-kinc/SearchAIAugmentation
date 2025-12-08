namespace PromptEnhancer.Models.Examples
{
    /// <summary>
    /// Represents a record containing a URL and its associated content.
    /// </summary>
    /// <remarks>This class is typically used to store and manage information about a URL and its
    /// corresponding content. Both the <see cref="Url"/> and <see cref="Content"/> properties are required and must be
    /// set.</remarks>
    public class UrlRecord
    {
        public required string Url { get; set; }

        /// <summary>
        /// Content associated with the URL.
        /// </summary>
        public required string Content { get; set; }
    }
}
