namespace Fanex.SessionStateProvider.RedisCore
{
    using Microsoft.Extensions.Options;

    public sealed class RedisCacheOptions : IOptions<RedisCacheOptions>
    {
        public string ApplicationName { get; set; }

        public string ConnectionString { get; set; }

        RedisCacheOptions IOptions<RedisCacheOptions>.Value => this;
    }
}