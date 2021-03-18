namespace Fanex.SessionStateProvider.AspNetCore
{
    using System;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Session;
    using Microsoft.Extensions.Logging;

    public class SharedSessionStore : ISessionStore
    {
        private readonly ICache _cache;
        private readonly ILoggerFactory _loggerFactory;

        public SharedSessionStore(ICache cache, ILoggerFactory loggerFactory)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        }

        public ISession Create(
            string sessionKey,
            TimeSpan idleTimeout,
            TimeSpan ioTimeout,
            Func<bool> tryEstablishSession,
            bool isNewSessionKey)
        {
            if (string.IsNullOrEmpty(sessionKey))
            {
                throw new ArgumentException("Argument cannot be null or empty string.", nameof(sessionKey));
            }

            if (tryEstablishSession == null)
            {
                throw new ArgumentNullException(nameof(tryEstablishSession));
            }

            return new SharedSession(
                _cache,
                sessionKey,
                idleTimeout,
                ioTimeout,
                tryEstablishSession,
                _loggerFactory,
                isNewSessionKey);
        }
    }
}