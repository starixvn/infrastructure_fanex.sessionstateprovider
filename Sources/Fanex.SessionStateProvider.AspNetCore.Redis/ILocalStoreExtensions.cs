namespace Fanex.SessionStateProvider.AspNetCore.Redis
{
    using System.Collections.Generic;
    using StackExchange.Redis;

    public static class ILocalStoreExtensions
    {
        public static byte[] GetValueWithoutMarkingModified(this ILocalStore store, string key)
        {
            var (_, value) = store.TryGetValueWithoutMarkingModified(key);

            return value;
        }

        public static List<RedisValue> ConvertToRedisValues(this ILocalStore store)
        {
            var result = new List<RedisValue>();

            foreach (string key in store.Keys)
            {
                result.Add(key);
                result.Add(store.GetValueWithoutMarkingModified(key));
            }

            return result;
        }

        public static (int NumberOfModifiedKeys, List<RedisValue> Result) ConvertModifiedItemsToRedisValues(this ILocalStore store)
        {
            var result = new List<RedisValue>();

            foreach (var key in store.ModifiedKeys)
            {
                result.Add(key);
                result.Add(store.GetValueWithoutMarkingModified(key));
            }

            return (store.ModifiedKeys.Count, result);
        }

        public static (int NumberOfDeletedKeys, List<RedisValue> Result) ConvertDeletedItemsToRedisValues(this ILocalStore store)
        {
            var result = new List<RedisValue>();

            foreach (var key in store.DeletedKeys)
            {
                result.Add(key);
            }

            return (store.DeletedKeys.Count, result);
        }
    }
}