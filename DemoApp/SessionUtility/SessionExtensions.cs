using Microsoft.AspNetCore.DataProtection;
using System.Text.Json;

namespace DemoApp.SessionUtility
{
    /// <summary>
    /// Provides extension methods for storing and retrieving objects in an <see cref="ISession"/> as JSON strings with
    /// data protection.
    /// </summary>
    /// <remarks>These methods use JSON serialization to convert objects to and from strings, and apply data
    /// protection to ensure the stored data is encrypted. This is useful for securely persisting complex objects in
    /// session storage.</remarks>
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
