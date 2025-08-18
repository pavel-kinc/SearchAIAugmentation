using Microsoft.AspNetCore.DataProtection;
using System.Text.Json;

namespace DemoApp.SessionUtility
{
    public static class SessionExtensions
    {
        private static readonly IDataProtectionProvider _provider = DataProtectionProvider.Create("ConfigSession");

        private static readonly IDataProtector _protector = _provider.CreateProtector("SessionJsonProtection");

        public static void SetObjectAsJson(this ISession session, string key, object value)
        {
            session.SetString(key, _protector.Protect(JsonSerializer.Serialize(value)));
        }

        public static T? GetObjectFromJson<T>(this ISession session, string key)
        {
            return session.GetString(key) is string json
                ? JsonSerializer.Deserialize<T>(_protector.Unprotect(json))
                : default;
        }
    }
}
