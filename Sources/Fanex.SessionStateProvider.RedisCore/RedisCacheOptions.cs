namespace Fanex.SessionStateProvider.RedisCore
{
    using System;

    public sealed class RedisCacheOptions
    {
        public string ApplicationName { get; set; }

        public string ConnectionString { get; set; }

        public TimeSpan RetryTimeout { get; set; }
    }
}