using DemoApp.Services.Interfaces;
using DemoApp.SessionUtility;
using PromptEnhancer.Models;

namespace DemoApp.Services
{
    public class EntrySetupService : IEntrySetupService
    {
        private readonly ISession _session;

        private const string SessionPrefix = "Entries.";

        public EntrySetupService(IHttpContextAccessor ctx)
        {
            _session = ctx.HttpContext!.Session;
            if (!_session.Keys.Any(k => k.StartsWith(SessionPrefix)))
            {
                SetEntriesToSession([]);
            }
        }

        public IEnumerable<Entry> GetEntries()
        {
            return GetEntriesFromSession();
        }

        public void AddEntry(Entry entry)
        {
            var entries = GetEntriesFromSession();
            entries!.Add(entry);
            SetEntriesToSession(entries);
        }

        public void UpdateEntries(IEnumerable<Entry> entries)
        {
            if (entries is not null && entries.Any())
            {
                SetEntriesToSession(entries.ToList());
            }
        }

        private List<Entry> GetEntriesFromSession()
        {
            return _session.GetObjectFromJson<List<Entry>>(SessionPrefix + nameof(Entry)) ?? [];
        }

        private void SetEntriesToSession(List<Entry> entries)
        {
            _session.SetObjectAsJson(SessionPrefix + nameof(Entry), entries);
        }
    }
}
