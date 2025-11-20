using PromptEnhancer.Models;

namespace DemoApp.Services.Interfaces
{
    public interface IEntrySetupService
    {
        IEnumerable<Entry> GetEntries();
        void AddEntry(Entry entry);
        void UpdateEntries(IEnumerable<Entry> entries);
    }
}
