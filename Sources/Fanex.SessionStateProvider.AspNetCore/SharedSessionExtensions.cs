namespace Fanex.SessionStateProvider.AspNetCore
{
    using Microsoft.AspNetCore.Http;

    public static class SharedSessionExtensions
    {
        private static readonly ISerializer _serializer = new TypelessMessagePackSerializer();

        public static void Set<T>(this ISession session, string key, T value)
        {
            session.Set(key, _serializer.Serialize(value));
        }

        public static T Get<T>(this ISession session, string key)
        {
            session.TryGetValue(key, out byte[] value);

            return value == null ? default : _serializer.Deserialize<T>(value);
        }
    }
}