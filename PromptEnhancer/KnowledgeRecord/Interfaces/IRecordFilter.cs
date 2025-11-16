namespace PromptEnhancer.KnowledgeRecord.Interfaces
{
    public interface IRecordFilter<T> : IRecordFilter
        where T : class
    {
        public IEnumerable<T> FilterRecords(IEnumerable<T> records);
    }

    public interface IRecordFilter
    {
    }
}
