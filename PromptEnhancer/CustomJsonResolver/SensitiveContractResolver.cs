using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

using PromptEnhancer.CustomAttributes;

namespace PromptEnhancer.CustomJsonResolver
{
    /// <summary>
    /// A custom contract resolver that excludes properties marked with the <see cref="SensitiveAttribute"/> from JSON
    /// serialization.
    /// </summary>
    public class SensitiveContractResolver : DefaultContractResolver
    {
        /// <summary>
        /// Creates a list of JSON properties for the specified type, excluding properties marked with the <see
        /// cref="SensitiveAttribute"/>.
        /// </summary>
        /// <remarks>This method overrides the base implementation to filter out properties that are
        /// marked with the <see cref="SensitiveAttribute"/>. Properties without a name or without the <see
        /// cref="SensitiveAttribute"/> are included in the returned list.</remarks>
        /// <param name="type">The type for which to create JSON properties.</param>
        /// <param name="memberSerialization">The member serialization options to use.</param>
        /// <returns>A list of <see cref="JsonProperty"/> objects representing the serializable properties of the specified type,
        /// excluding those marked as sensitive.</returns>
        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            return base.CreateProperties(type, memberSerialization)
                .Where(p =>
                {
                    if (string.IsNullOrEmpty(p.PropertyName))
                    {
                        return true;
                    }

                    var propInfo = type.GetProperty(p.PropertyName);
                    return propInfo == null || !propInfo.IsDefined(typeof(SensitiveAttribute), true);
                })
                .ToList();
        }
    }
}
