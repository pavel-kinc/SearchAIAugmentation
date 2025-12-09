namespace PromptEnhancer.CustomAttributes
{
    /// <summary>
    /// Indicates that a property contains sensitive information.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class SensitiveAttribute : Attribute
    {
    }
}
