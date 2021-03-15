namespace Fanex.SessionStateProvider.Redis
{
    internal static class RedisScripts
    {
        public const string UpdateExpiryTime =
            @"local dataExists = redis.call('EXISTS', KEYS[1])

              if dataExists == 0 then
                  return 1;
              end

              local sessionTimeout = redis.call('HGET', KEYS[2], 'SessionTimeout')

              if sessionTimeout ~= false then
                  redis.call('EXPIRE', KEYS[1], sessionTimeout)
                  redis.call('EXPIRE', KEYS[2], sessionTimeout)
              else
                  redis.call('EXPIRE', KEYS[1], ARGV[1])
                  redis.call('HSET', KEYS[2], 'SessionTimeout', ARGV[1])
                  redis.call('EXPIRE', KEYS[2], ARGV[1])
              end

              return 1";

        public const string WriteData =
            @"redis.call('HMSET', KEYS[1], unpack(ARGV, 4, ARGV[1]))
              redis.call('EXPIRE', KEYS[1], ARGV[2])
              redis.call('HMSET', KEYS[2], 'SessionTimeout', ARGV[2], 'SessionStateActions', ARGV[3])
              redis.call('EXPIRE', KEYS[2], ARGV[2])

              return 1";

        public const string ObtainLockThenReadData =
            @"local result = {}
              local lockId = ARGV[1]
              local obtainingLock = redis.call('SETNX', KEYS[1], ARGV[1])
              local locked = false
              local sessionData = false

              if obtainingLock == 0 then
                  lockId = redis.call('GET', KEYS[1])
                  locked = true
              else
                  redis.call('EXPIRE', KEYS[1], ARGV[2])
                  sessionData = redis.call('HGETALL', KEYS[2])
              end

              result[1] = lockId
              result[2] = sessionData

              local internalData = redis.call('HMGET', KEYS[3], 'SessionTimeout', 'SessionStateActions')
              local sessionTimeout = internalData[1]
              local sessionStateActions = internalData[2]

              if sessionTimeout ~= false then
                  redis.call('EXPIRE', KEYS[2], sessionTimeout)
                  redis.call('EXPIRE', KEYS[3], sessionTimeout)
              end

              result[3] = sessionTimeout
              result[4] = sessionStateActions
              result[5] = locked

              return result";

        public const string ReadData =
            @"local result = {}
              local lockId = redis.call('GET', KEYS[1])
              local locked = false

              result[1] = lockId

              if lockId ~= false then
                  result[2] = redis.call('HGETALL', KEYS[2])
              else
                  result[2] = false
                  locked = true
              end

              local internalData = redis.call('HMGET', KEYS[3], 'SessionTimeout', 'SessionStateActions')
              local sessionTimeout = internalData[1]
              local sessionStateActions = internalData[2]

              if sessionTimeout ~= false then
                  redis.call('EXPIRE', KEYS[2], sessionTimeout)
                  redis.call('EXPIRE', KEYS[3], sessionTimeout)
              end

              result[3] = sessionTimeout
              result[4] = sessionStateActions
              result[5] = locked

              return result";

        public const string ReleaseLock =
            @"local lockId = redis.call('GET', KEYS[1])

              if lockId == ARGV[1] then
                  redis.call('DEL',KEYS[1])
              end

              local sessionTimeout = redis.call('HGET', KEYS[3], 'SessionTimeout')

              if sessionTimeout ~= false then
                  redis.call('EXPIRE', KEYS[2], sessionTimeout)
                  redis.call('EXPIRE', KEYS[3], sessionTimeout)
              else
                  redis.call('EXPIRE', KEYS[2], ARGV[2])
              end

              return 1";

        public const string DeleteData =
            @"local lockId = redis.call('GET', KEYS[1])

              if lockId ==  ARGV[1] then
                  redis.call('DEL', KEYS[2])
                  redis.call('DEL', KEYS[3])
                  redis.call('DEL', KEYS[1])
              end

              return 1";

        public const string ChangeData =
            @"local lockId = redis.call('GET', KEYS[1])

              if lockId == ARGV[1] then
                  if tonumber(ARGV[4]) ~= 0 then
                      redis.call('HDEL', KEYS[2], unpack(ARGV, ARGV[5], ARGV[6]))
                  end

                  if tonumber(ARGV[7]) ~= 0 then
                      redis.call('HMSET', KEYS[2], unpack(ARGV, ARGV[8], ARGV[9]))
                  end

                  redis.call('EXPIRE', KEYS[2], ARGV[2])
                  redis.call('HMSET', KEYS[3], 'SessionTimeout', ARGV[2], 'SessionStateActions', ARGV[3])
                  redis.call('EXPIRE', KEYS[3], ARGV[2])
                  redis.call('DEL', KEYS[1])
              end

              return 1";
    }
}