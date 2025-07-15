using PromptEnhancer.Models;

namespace DemoApp.Services.Interfaces
{
    public interface IEntrySetupService
    {
        IEnumerable<Entry> GetEntries();
        void UpdateEntry(IEnumerable<Entry> entries);
        public void AddEntry(Entry entry);
    }
}
