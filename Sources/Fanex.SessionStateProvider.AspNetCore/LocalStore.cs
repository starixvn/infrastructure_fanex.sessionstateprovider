namespace Fanex.SessionStateProvider.AspNetCore
{
    using System;
    using System.Collections;
    using System.Collections.Concurrent;
    using System.Collections.Generic;

    public sealed class LocalStore : ILocalStore
    {
        private readonly ConcurrentDictionary<string, byte[]> _store = new(StringComparer.InvariantCultureIgnoreCase);
        private readonly HashSet<string> _modifiedKeys = new(StringComparer.InvariantCultureIgnoreCase);
        private readonly HashSet<string> _deletedKeys = new(StringComparer.InvariantCultureIgnoreCase);

        public int Count => _store.Count;

        public ICollection<string> Keys => _store.Keys;

        public ICollection<string> ModifiedKeys => _modifiedKeys;

        public ICollection<string> DeletedKeys => _deletedKeys;

        public void Clear()
        {
            foreach (string key in _store.Keys)
            {
                MarkAsDeletedKey(key);
            }

            _store.Clear();
        }

        public IEnumerator<KeyValuePair<string, byte[]>> GetEnumerator()
            => _store.GetEnumerator();

        public bool Remove(string key)
        {
            if (_store.ContainsKey(key))
            {
                MarkAsDeletedKey(key);
            }

            return _store.TryRemove(key, out _);
        }

        public void SetValue(string key, byte[] value)
        {
            MarkAsModifiedKey(key);
            _store[key] = value;
        }

        public void SetValueWithoutMarkingModified(string key, byte[] value)
            => _store[key] = value;

        public (bool Existing, byte[] Value) TryGetValue(string key)
        {
            var existing = _store.TryGetValue(key, out byte[] value);

            if (IsReferenceType(value))
            {
                MarkAsModifiedKey(key);
            }

            return (existing, value);
        }

        public (bool Existing, byte[] Value) TryGetValueWithoutMarkingModified(string key)
        {
            var existing = _store.TryGetValue(key, out byte[] value);

            return (existing, value);
        }

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        private void MarkAsModifiedKey(string key)
        {
            DeletedKeys.Remove(key);
            ModifiedKeys.Add(key);
        }

        private void MarkAsDeletedKey(string key)
        {
            ModifiedKeys.Remove(key);
            DeletedKeys.Add(key);
        }

        private static bool IsReferenceType(object value)
            => value != null && !value.GetType().IsValueType && !(value is string);
    }
}