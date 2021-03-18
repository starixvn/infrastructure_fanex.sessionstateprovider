namespace Fanex.SessionStateProvider.AspNetCore.Redis
{
    using System.Collections.Generic;
    using StackExchange.Redis;

    public static class ILocalStoreExtensions
    {
        public static (int NumberOfModifiedKeys, List<RedisValue> Result) ConvertModifiedItemsToRedisValues(this ILocalStore store)
        {
            var result = new List<RedisValue>();

            foreach (var key in store.ModifiedKeys)
            {
                var (_, value) = store.TryGetValueWithoutMarkingModified(key);

                result.Add(key);
                result.Add(value);
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