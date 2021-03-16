namespace Fanex.SessionStateProvider.Redis.Extensions
{
    using System.Collections.Generic;
    using StackExchange.Redis;

    public static class ChangeTrackingSessionStateItemCollectionExtensions
    {
        public static List<RedisValue> ConvertToRedisValues(
            this ChangeTrackingSessionStateItemCollection sessionData,
            ISerializer serializer)
        {
            var result = new List<RedisValue>();

            foreach (string key in sessionData.Keys)
            {
                result.Add(key);
                result.Add(serializer.Serialize(sessionData.GetDataWithoutMarkingModified(key)));
            }

            return result;
        }

        public static (int NumberOfModifiedKeys, List<RedisValue> Result) ConvertModifiedItemsToRedisValues(
            this ChangeTrackingSessionStateItemCollection sessionData,
            ISerializer serializer)
        {
            var result = new List<RedisValue>();

            foreach (var key in sessionData.ModifiedKeys)
            {
                result.Add(key);
                result.Add(serializer.Serialize(sessionData.GetDataWithoutMarkingModified(key)));
            }

            return (sessionData.ModifiedKeys.Count, result);
        }

        public static (int NumberOfDeletedKeys, List<RedisValue> Result) ConvertDeletedItemsToRedisValues(this ChangeTrackingSessionStateItemCollection sessionData)
        {
            var result = new List<RedisValue>();

            foreach (var key in sessionData.DeletedKeys)
            {
                result.Add(key);
            }

            return (sessionData.DeletedKeys.Count, result);
        }
    }
}