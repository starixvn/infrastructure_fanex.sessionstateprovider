namespace Fanex.SessionStateProvider.AspNetCore.StateServer
{
    using System;
    using System.Net.Http.Headers;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Session;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Microsoft.Extensions.Options;

    public static class SessionServiceCollectionExtensions
    {
        public static IServiceCollection AddStateServerSession(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.AddSingleton<ISerializer, MessagePackSerializer>();
            services.AddHttpClient<ICache, StateServerCache>((serviceProvider, httpClient) =>
            {
                var stateServerCacheOptions = serviceProvider.GetRequiredService<IOptions<StateServerCacheOptions>>().Value;

                httpClient.BaseAddress = stateServerCacheOptions.ServiceUri;
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(StateServerCache.MimeType));
            });
            services.TryAddTransient<ISessionStore, SharedSessionStore>();
            services.AddDataProtection();

            return services;
        }

        public static IServiceCollection AddStateServerSession(
            this IServiceCollection services,
            Action<StateServerCacheOptions> configureStateServerService,
            Action<SessionOptions> configureSession)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (configureStateServerService == null)
            {
                throw new ArgumentNullException(nameof(configureStateServerService));
            }

            if (configureSession == null)
            {
                throw new ArgumentNullException(nameof(configureSession));
            }

            services.Configure(configureStateServerService);
            services.Configure(configureSession);
            services.AddStateServerSession();

            return services;
        }
    }
}