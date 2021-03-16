namespace Fanex.SessionStateProvider.RedisCore
{
    internal static class RedisScripts
    {
        public const string RefreshSessionTimeout =
            @"local dataExists = redis.call('EXISTS', KEYS[1])

              if dataExists == 0 then
                  return 1;
              end

              local sessionTimeout = redis.call('HGET', KEYS[2], 'SessionTimeout')

              if sessionTimeout ~= false then
                  redis.call('EXPIRE', KEYS[1], sessionTimeout)
                  redis.call('EXPIRE', KEYS[2], sessionTimeout)
              end

              return 1";

        public const string GetData =
            @"local result = {}
              local lockId = redis.call('GET', KEYS[1])

              result[1] = lockId

              if lockId ~= false then
                  result[2] = redis.call('HGETALL', KEYS[2])
              else
                  result[2] = false
              end

              local sessionTimeout = redis.call('HGET', KEYS[3], 'SessionTimeout')

              if sessionTimeout ~= false then
                  redis.call('EXPIRE', KEYS[2], sessionTimeout)
                  redis.call('EXPIRE', KEYS[3], sessionTimeout)
              end

              return result";

        public const string SetData =
            @"local lockId = redis.call('GET', KEYS[1])

              if lockId == ARGV[1] then
                  if tonumber(ARGV[3]) ~= 0 then
                      redis.call('HDEL', KEYS[2], unpack(ARGV, ARGV[4], ARGV[5]))
                  end

                  if tonumber(ARGV[6]) ~= 0 then
                      redis.call('HMSET', KEYS[2], unpack(ARGV, ARGV[7], ARGV[8]))
                  end

                  redis.call('EXPIRE', KEYS[2], ARGV[2])
                  redis.call('HMSET', KEYS[3], 'SessionTimeout', ARGV[2])
                  redis.call('EXPIRE', KEYS[3], ARGV[2])
                  redis.call('DEL', KEYS[1])

                  return 1;
              end

              return 0";
    }
}