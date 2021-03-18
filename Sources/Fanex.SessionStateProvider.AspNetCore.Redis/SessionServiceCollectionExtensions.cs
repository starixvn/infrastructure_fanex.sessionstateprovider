namespace Fanex.SessionStateProvider.AspNetCore.Redis
{
    using System;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Session;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;

    public static class SessionServiceCollectionExtensions
    {
        public static IServiceCollection AddRedisSession(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.TryAddSingleton<ICache, RedisCache>();
            services.TryAddTransient<ISessionStore, SharedSessionStore>();
            services.AddDataProtection();

            return services;
        }

        public static IServiceCollection AddRedisSession(
            this IServiceCollection services,
            Action<RedisCacheOptions> configureRedis,
            Action<SessionOptions> configureSession)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (configureRedis == null)
            {
                throw new ArgumentNullException(nameof(configureRedis));
            }

            if (configureSession == null)
            {
                throw new ArgumentNullException(nameof(configureSession));
            }

            services.Configure(configureRedis);
            services.Configure(configureSession);
            services.AddRedisSession();

            return services;
        }
    }
}