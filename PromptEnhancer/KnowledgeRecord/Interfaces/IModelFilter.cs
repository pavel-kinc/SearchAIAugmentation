namespace PromptEnhancer.KnowledgeRecord.Interfaces
{
    /// <summary>
    /// Defines a mechanism for filtering a collection of data of a specified type.
    /// </summary>
    /// <remarks>Implementations of this interface should provide logic to filter the input collection based
    /// on specific criteria. The filtering operation does not modify the original collection but returns a new
    /// collection containing the filtered results.</remarks>
    /// <typeparam name="T">The type of model data to be filtered. Must be a reference type.</typeparam>
    public interface IModelFilter<T> : IModelFilter
        where T : class
    {
        public IEnumerable<T> FilterModelData(IEnumerable<T> records);
    }

    /// <summary>
    /// Defines a contract for filtering models based on specific criteria.
    /// </summary>
    public interface IModelFilter
    {
    }

    /// <summary>
    /// Provides a default implementation of the <see cref="IModelFilter{T}"/> interface that returns the input data
    /// without applying any filtering.
    /// </summary>
    /// <typeparam name="T">The type of the model data to be filtered. Must be a reference type.</typeparam>
    public class EmptyModelFilter<T> : IModelFilter<T>
        where T : class
    {
        public virtual IEnumerable<T> FilterModelData(IEnumerable<T> records)
        {
            return records;
        }
    }
}
