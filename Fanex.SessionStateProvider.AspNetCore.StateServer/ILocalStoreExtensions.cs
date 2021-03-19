namespace Fanex.SessionStateProvider.AspNetCore.StateServer
{
    using System;
    using System.Collections.Generic;

    public static class ILocalStoreExtensions
    {
        public static (int NumberOfModifiedKeys, Dictionary<string, byte[]> Result) GetModifiedItems(this ILocalStore store)
        {
            var result = new Dictionary<string, byte[]>(StringComparer.InvariantCultureIgnoreCase);

            foreach (var key in store.ModifiedKeys)
            {
                var (_, value) = store.TryGetValueWithoutMarkingModified(key);

                result[key] = value;
            }

            return (store.ModifiedKeys.Count, result);
        }

        public static (int NumberOfDeletedKeys, List<string> Result) GetDeletedKeys(this ILocalStore store)
        {
            var result = new List<string>();

            foreach (var key in store.DeletedKeys)
            {
                result.Add(key);
            }

            return (store.DeletedKeys.Count, result);
        }
    }
}