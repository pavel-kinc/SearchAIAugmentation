using PromptEnhancer.CustomAttributes;
using PromptEnhancer.Models.Configurations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PromptEnhancer.ConfigurationHelper
{
    public static class SensitiveDataResolver
    {
        public static void HideSensitiveProperties(object obj)
        {
            if (obj == null)
            {
                return;
            }

            var type = obj.GetType();
            var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in props)
            {
                if (!prop.CanRead || !prop.CanWrite)
                    continue;

                var value = prop.GetValue(obj);
                var propType = prop.PropertyType;

                if (Attribute.IsDefined(prop, typeof(SensitiveAttribute)))
                {
                    var defaultValue = propType.IsValueType ? Activator.CreateInstance(propType) : null;
                    prop.SetValue(obj, defaultValue);
                }
                else if (value != null && !propType.IsPrimitive && propType != typeof(string))
                {
                    HideSensitiveProperties(value);
                }
            }
        }
    }
}
