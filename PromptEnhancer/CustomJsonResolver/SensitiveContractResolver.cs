using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

using PromptEnhancer.CustomAttributes;

namespace PromptEnhancer.CustomJsonResolver
{
    public class SensitiveContractResolver : DefaultContractResolver
    {
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
