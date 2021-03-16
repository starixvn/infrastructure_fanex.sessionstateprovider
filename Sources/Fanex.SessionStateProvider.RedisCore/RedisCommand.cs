namespace Fanex.SessionStateProvider.RedisCore
{
    using System.Collections.Generic;
    using System.Linq;
    using Fanex.SessionStateProvider.RedisCore.Extensions;
    using StackExchange.Redis;

    public class RedisCommand
    {
        private RedisCommand(
            string script,
            RedisKey[] keys,
            RedisValue[] values)
        {
            Keys = keys;
            Values = values;
            Script = script;
        }

        public string Script { get; }

        public RedisKey[] Keys { get; }

        public RedisValue[] Values { get; }

        public static RedisCommand CreateRefreshSessionTimeoutCommand(SessionKeySet keySet)
            => new(
                RedisScripts.RefreshSessionTimeout,
                new RedisKey[]
                {
                    keySet.DataKey,
                    keySet.InternalKey
                },
                new RedisValue[0]);

        public static RedisCommand CreateGetCommand(SessionKeySet keySet)
            => new(
                RedisScripts.GetData,
                new RedisKey[]
                {
                    keySet.DataKey,
                    keySet.InternalKey
                },
                new RedisValue[0]);

        public static RedisCommand CreateSetCommand(
            SessionKeySet keySet,
            object lockId,
            ILocalStore store,
            int sessionTimeout)
        {
            if (store == null || store.Count == 0)
            {
                return null;
            }

            const int metadataLength = 8;
            const int metadataLastIndexInLua = 9;

            var (numberOfDeletedItems, deletedItems) = store.ConvertDeletedItemsToRedisValues();
            var (numberOfModifiedItems, modifiedItems) = store.ConvertModifiedItemsToRedisValues();
            var changedItems = deletedItems.Concat(modifiedItems).ToList();
            var keys = new RedisKey[]
            {
                keySet.DataKey,
                keySet.InternalKey
            };
            var values = new List<RedisValue>
            {
                (long)lockId,
                sessionTimeout,
                numberOfDeletedItems,
                metadataLastIndexInLua, // The first position of deleted items in the argument list. LUA script's list is 1-based index, then it goes 9 insted of 8.
                numberOfDeletedItems + metadataLength, // The last position of deleted items in the argument list.
                numberOfModifiedItems,
                numberOfDeletedItems + metadataLastIndexInLua, // The first position of modified/new items in the argument list.
                changedItems.Count + metadataLength // The last position of modified/new items in the argument list.
            };

            if (changedItems.Count > 0)
            {
                values.AddRange(changedItems);
            }

            return new RedisCommand(RedisScripts.SetData, keys, values.ToArray());
        }
    }
}