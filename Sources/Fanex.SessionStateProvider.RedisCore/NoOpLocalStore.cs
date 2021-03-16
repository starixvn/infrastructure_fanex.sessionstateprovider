namespace Fanex.SessionStateProvider.RedisCore
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    internal class NoOpLocalStore : ILocalStore
    {
        public int Count => 0;

        public ICollection<string> Keys => Array.Empty<string>();

        public ICollection<string> ModifiedKeys => Array.Empty<string>();

        public ICollection<string> DeletedKeys => Array.Empty<string>();

        public void Clear()
        {
            // Since it is a NoOp store, we have nothing to do with this method.
        }

        public IEnumerator<KeyValuePair<string, byte[]>> GetEnumerator()
            => Enumerable.Empty<KeyValuePair<string, byte[]>>().GetEnumerator();

        public bool Remove(string key)
            => false;

        public void SetValue(string key, byte[] value)
        {
            // Since it is a NoOp store, we have nothing to do with this method.
        }

        public void SetValueWithoutMarkingModified(string key, byte[] value)
        {
            // Since it is a NoOp store, we have nothing to do with this method.
        }

        public (bool Existing, byte[] Value) TryGetValueWithoutMarkingModified(string key)
            => (false, null);

        public (bool Existing, byte[] Value) TryGetValue(string key)
            => (false, null);

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();
    }
}