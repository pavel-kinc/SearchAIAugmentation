using PromptEnhancer.Models;

namespace DemoApp.Services.Interfaces
{
    /// <summary>
    /// Provides methods for managing and retrieving entries in the system.
    /// </summary>
    /// <remarks>This interface defines operations for retrieving, adding, and updating entries. 
    /// Implementations of this interface are responsible for ensuring the integrity of the entry data.</remarks>
    public interface IEntrySetupService
    {
        /// <summary>
        /// Retrieves a collection of entries for user.
        /// </summary>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="Entry"/> objects representing the entries. The collection may
        /// be empty if no entries are available.</returns>
        IEnumerable<Entry> GetEntries();

        /// <summary>
        /// Adds a new entry to the collection.
        /// </summary>
        /// <param name="entry">The entry to add. Cannot be <see langword="null"/>.</param>
        void AddEntry(Entry entry);

        /// <summary>
        /// Updates the specified collection of entries for user.
        /// </summary>
        /// <remarks>This method processes the provided entries and applies updates to the system. Ensure
        /// that the entries are properly validated before calling this method to avoid errors.</remarks>
        /// <param name="entries">A collection of <see cref="Entry"/> objects to be updated. Each entry must contain valid data. The
        /// collection cannot be null.</param>
        void UpdateEntries(IEnumerable<Entry> entries);
    }
}
