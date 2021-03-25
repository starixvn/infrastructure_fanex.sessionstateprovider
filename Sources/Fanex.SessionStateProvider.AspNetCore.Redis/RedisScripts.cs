namespace Fanex.SessionStateProvider.AspNetCore.Redis
{
    internal static class RedisScripts
    {
        public const string RefreshSessionTimeout =
            @"local dataExists = redis.call('EXISTS', KEYS[1])

              if dataExists == 0 then
                  return 1
              end

              local sessionTimeout = redis.call('HGET', KEYS[2], 'SessionTimeout')

              if sessionTimeout ~= false then
                  redis.call('EXPIRE', KEYS[1], sessionTimeout)
                  redis.call('EXPIRE', KEYS[2], sessionTimeout)
              end

              return 1";

        public const string GetData =
            @"local result = {}
              local data = redis.call('HGETALL', KEYS[1])
              local internalData = redis.call('HMGET', KEYS[2], 'AspNetCoreSessionId', 'SessionTimeout')
              local sessionId = internalData[1]
              local sessionTimeout = internalData[2]

              if sessionTimeout ~= false then
                  redis.call('EXPIRE', KEYS[1], sessionTimeout)
                  redis.call('EXPIRE', KEYS[2], sessionTimeout)
              end

              result[1] = sessionId
              result[2] = data

              return result";

        public const string SetData =
            @"if tonumber(ARGV[3]) ~= 0 then
                  redis.call('HDEL', KEYS[1], unpack(ARGV, ARGV[4], ARGV[5]))
              end

              if tonumber(ARGV[6]) ~= 0 then
                  redis.call('HMSET', KEYS[1], unpack(ARGV, ARGV[7], ARGV[8]))
              end

              redis.call('EXPIRE', KEYS[1], ARGV[2])
              redis.call('HMSET', KEYS[2], 'AspNetCoreSessionId', ARGV[1], 'SessionTimeout', ARGV[2])
              redis.call('EXPIRE', KEYS[2], ARGV[2])

              return 1";
    }
}