using DemoApp.Services.Interfaces;
using Mapster;
using PromptEnhancer.Models;

namespace DemoApp.Services
{
    public class EntrySetupService : IEntrySetupService
    {
        private List<Entry> _entries = [];

        public IEnumerable<Entry> GetEntries()
        {
            return _entries.Adapt<IEnumerable<Entry>>();
        }

        public void AddEntry(Entry entry)
        {
            _entries.Add(entry);
        }

        public void UpdateEntries(IEnumerable<Entry> entries)
        {
            if (entries is not null && entries.Any())
            {
                _entries = entries.ToList();
            }
        }
    }
}
