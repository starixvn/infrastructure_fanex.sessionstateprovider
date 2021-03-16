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

        public static IServiceCollection AddSession(this IServiceCollection services, Action<SessionOptions> configure)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            services.Configure(configure);
            services.AddSession();

            return services;
        }
    }
}