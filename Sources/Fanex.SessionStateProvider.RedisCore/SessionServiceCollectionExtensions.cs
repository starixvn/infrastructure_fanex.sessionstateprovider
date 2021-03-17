namespace Fanex.SessionStateProvider.RedisCore
{
    using System;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Session;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;

    public static class SessionServiceCollectionExtensions
    {
        public static IServiceCollection AddSession(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.TryAddSingleton<ICache, RedisCache>();
            services.TryAddTransient<ISessionStore, RedisSessionStore>();
            services.AddDataProtection();

            return services;
        }

        public static IServiceCollection AddSession(
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
            services.AddSession();

            return services;
        }
    }
}