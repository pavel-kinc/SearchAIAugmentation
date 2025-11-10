namespace PromptEnhancer.KnowledgeRecord.Interfaces
{
    public interface IRecordFilter<T>
        where T : class
    {
        public IEnumerable<T> FilterRecords(IEnumerable<T> records);
    }
}
