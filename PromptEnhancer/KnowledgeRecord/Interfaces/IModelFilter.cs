namespace PromptEnhancer.KnowledgeRecord.Interfaces
{
    public interface IModelFilter<T> : IModelFilter
        where T : class
    {
        public IEnumerable<T> FilterRecords(IEnumerable<T> records);
    }

    public interface IModelFilter
    {
    }

    public class EmptyModelFilter<T> : IModelFilter<T>
        where T : class
    {
        public virtual IEnumerable<T> FilterRecords(IEnumerable<T> records)
        {
            return records;
        }
    }
}
