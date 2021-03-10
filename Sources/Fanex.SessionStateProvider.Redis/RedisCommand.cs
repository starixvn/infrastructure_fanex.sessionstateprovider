namespace Fanex.SessionStateProvider.Redis
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.SessionState;
    using Fanex.SessionStateProvider.Redis.Extensions;
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

        public static RedisCommand CreateUpdateExpiryTimeCommand(SessionKeySet keySet, int timeToExpireInSeconds)
            => new(
                RedisScripts.UpdateExpiryTime,
                new RedisKey[]
                {
                    keySet.DataKey,
                    keySet.InternalKey
                },
                new RedisValue[]
                {
                    timeToExpireInSeconds
                });

        public static RedisCommand CreateWriteDataCommand(
            SessionKeySet keySet,
            ChangeTrackingSessionStateItemCollection sessionData,
            int sessionTimeout,
            SessionStateActions actions,
            ISerializer serializer)
        {
            if (sessionData == null || sessionData.Count == 0)
            {
                return null;
            }

            const int metadataLength = 3;

            var changedItems = sessionData.ConvertToRedisValues(serializer);
            var keys = new RedisKey[]
            {
                keySet.DataKey,
                keySet.InternalKey
            };
            var values = new List<RedisValue>
            {
                changedItems.Count + metadataLength,
                sessionTimeout,
                (int)actions
            };

            if (changedItems.Count > 0)
            {
                values.AddRange(changedItems);
            }

            return new RedisCommand(RedisScripts.WriteData, keys, values.ToArray());
        }

        public static RedisCommand CreateObtainLockThenReadDataCommand(SessionKeySet keySet, DateTimeOffset lockTime, int lockTimeout)
            => new(
                RedisScripts.ObtainLockThenReadData,
                new RedisKey[]
                {
                    keySet.LockKey,
                    keySet.DataKey,
                    keySet.InternalKey
                },
                new RedisValue[]
                {
                    lockTime.Ticks,
                    lockTimeout
                });

        public static RedisCommand CreateReadDataCommand(SessionKeySet keySet)
            => new(
                RedisScripts.ReadData,
                new RedisKey[]
                {
                    keySet.LockKey,
                    keySet.DataKey,
                    keySet.InternalKey
                },
                new RedisValue[0]);

        public static RedisCommand CreateReleaseLockCommand(SessionKeySet keySet, object lockId, int sessionTimeout)
            => new(
                RedisScripts.ReleaseLock,
                new RedisKey[]
                {
                    keySet.LockKey,
                    keySet.DataKey,
                    keySet.InternalKey
                },
                new RedisValue[]
                {
                    (long)lockId,
                    sessionTimeout
                });

        public static RedisCommand CreateDeleteDataCommand(SessionKeySet keySet, object lockId)
            => new(
                RedisScripts.DeleteData,
                new RedisKey[]
                {
                    keySet.LockKey,
                    keySet.DataKey,
                    keySet.InternalKey
                },
                new RedisValue[]
                {
                    (long)lockId
                });

        public static RedisCommand CreateChangeDataCommand(
            SessionKeySet keySet,
            object lockId,
            ChangeTrackingSessionStateItemCollection sessionData,
            int sessionTimeout,
            ISerializer serializer)
        {
            if (sessionData == null || sessionData.Count == 0)
            {
                return null;
            }

            const int metadataLength = 9;
            const int metadataLastIndexInLua = 10;

            var (numberOfDeletedItems, deletedItems) = sessionData.ConvertDeletedItemsToRedisValues();
            var (numberOfModifiedItems, modifiedItems) = sessionData.ConvertModifiedItemsToRedisValues(serializer);
            var changedItems = deletedItems.Concat(modifiedItems).ToList();
            var keys = new RedisKey[]
            {
                keySet.LockKey,
                keySet.DataKey,
                keySet.InternalKey
            };
            var values = new List<RedisValue>
            {
                (long)lockId,
                sessionTimeout,
                (int)SessionStateActions.None,
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

            return new RedisCommand(RedisScripts.ChangeData, keys, values.ToArray());
        }
    }
}