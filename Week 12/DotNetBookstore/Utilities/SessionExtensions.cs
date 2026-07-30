using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace DotNetBookstore.Utilities
{
    /// <summary>
    /// Simple session extension helpers for storing complex objects using JSON.
    /// ASP.NET Core's ISession only accepts byte[]; complex types must be
    /// serialized. This mirrors the pattern recommended in the docs and keeps
    /// controllers small and readable for the classroom demo.
    /// </summary>
    public static class SessionExtensions
    {
        // Store an object as JSON in session
        public static void SetObject<T>(this ISession session, string key, T value)
        {
            var json = JsonSerializer.Serialize(value);
            session.SetString(key, json);
        }

        // Retrieve an object from session or return default(T) if missing
        public static T? GetObject<T>(this ISession session, string key)
        {
            var json = session.GetString(key);
            if (string.IsNullOrEmpty(json))
                return default;
            return JsonSerializer.Deserialize<T>(json);
        }
    }
}