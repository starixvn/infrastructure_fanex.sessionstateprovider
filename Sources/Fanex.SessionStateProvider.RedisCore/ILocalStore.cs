namespace Fanex.SessionStateProvider.RedisCore
{
    using System.Collections.Generic;

    public interface ILocalStore : IEnumerable<KeyValuePair<string, byte[]>>
    {
        int Count { get; }

        ICollection<string> Keys { get; }

        ICollection<string> ModifiedKeys { get; }

        ICollection<string> DeletedKeys { get; }

        (bool Existing, byte[] Value) TryGetValue(string key);

        (bool Existing, byte[] Value) TryGetValueWithoutMarkingModified(string key);

        void SetValue(string key, byte[] value);

        void SetValueWithoutMarkingModified(string key, byte[] value);

        bool Remove(string key);

        void Clear();
    }
}