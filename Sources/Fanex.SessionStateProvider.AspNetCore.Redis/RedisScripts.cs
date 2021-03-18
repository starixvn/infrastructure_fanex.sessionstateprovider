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
            @"local result = redis.call('HGETALL', KEYS[1])

              local sessionTimeout = redis.call('HGET', KEYS[2], 'SessionTimeout')

              if sessionTimeout ~= false then
                  redis.call('EXPIRE', KEYS[1], sessionTimeout)
                  redis.call('EXPIRE', KEYS[2], sessionTimeout)
              end

              return result";

        public const string SetData =
            @"if tonumber(ARGV[2]) ~= 0 then
                  redis.call('HDEL', KEYS[1], unpack(ARGV, ARGV[3], ARGV[4]))
              end

              if tonumber(ARGV[5]) ~= 0 then
                  redis.call('HMSET', KEYS[1], unpack(ARGV, ARGV[6], ARGV[7]))
              end

              redis.call('EXPIRE', KEYS[1], ARGV[1])
              redis.call('HMSET', KEYS[2], 'SessionTimeout', ARGV[1])
              redis.call('EXPIRE', KEYS[2], ARGV[1])

              return 1";
    }
}