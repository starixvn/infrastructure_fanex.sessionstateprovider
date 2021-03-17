namespace Fanex.SessionStateProvider.RedisCore
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;

    public class RedisSession : ISession
    {
        private readonly ICache _cache;
        private readonly string _sessionKey;
        private readonly TimeSpan _idleTimeout;
        private readonly TimeSpan _ioTimeout;
        private readonly Func<bool> _tryEstablishSession;
        private readonly ILogger _logger;
        private readonly bool _isNewSessionKey;

        private ILocalStore _store;

        private string _sessionId;
        private bool _loaded;
        private bool _isAvailable;
        private bool _isModified;

        public RedisSession(
            ICache cache,
            string sessionKey,
            TimeSpan idleTimeout,
            TimeSpan ioTimeout,
            Func<bool> tryEstablishSession,
            ILoggerFactory loggerFactory,
            bool isNewSessionKey)
        {
            if (string.IsNullOrEmpty(sessionKey))
            {
                throw new ArgumentException("Argument cannot be null or empty string.", nameof(sessionKey));
            }

            if (loggerFactory == null)
            {
                throw new ArgumentNullException(nameof(loggerFactory));
            }

            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _sessionKey = sessionKey;
            _idleTimeout = idleTimeout;
            _ioTimeout = ioTimeout;
            _tryEstablishSession = tryEstablishSession ?? throw new ArgumentNullException(nameof(tryEstablishSession));
            _logger = loggerFactory.CreateLogger<RedisSession>();

            _isNewSessionKey = isNewSessionKey;

            _store = new LocalStore();
        }

        public bool IsAvailable => GetAvailableStatus();

        public string Id => GetSessionId();

        public IEnumerable<string> Keys => GetKeys();

        private string GetSessionId()
        {
            Load();

            _sessionId ??= Guid.NewGuid().ToString();

            return _sessionId;
        }

        private bool GetAvailableStatus()
        {
            Load();

            return _isAvailable;
        }

        private IEnumerable<string> GetKeys()
        {
            Load();

            return _store.Keys;
        }

        public void Clear()
        {
            Load();

            _isModified |= _store.Count > 0;
            _store.Clear();
        }

        public async Task CommitAsync(CancellationToken cancellationToken = default)
        {
            if (!IsAvailable)
            {
                _logger.SessionNotAvailable();

                return;
            }

            using var ioTimeoutTokenSource = new CancellationTokenSource(_ioTimeout);
            var accessingCacheTokenSource = CancellationTokenSource.CreateLinkedTokenSource(ioTimeoutTokenSource.Token, cancellationToken);

            if (_isModified)
            {
                await CommitModifiedDataAsync(ioTimeoutTokenSource, accessingCacheTokenSource);
            }
            else
            {
                await RefreshAsync(ioTimeoutTokenSource, accessingCacheTokenSource);
            }
        }

        private async Task CommitModifiedDataAsync(CancellationTokenSource ioTimeoutTokenSource, CancellationTokenSource accessingCacheTokenSource)
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                await TryLogSessionStartedAsync(accessingCacheTokenSource);
            }

            try
            {
                accessingCacheTokenSource.Token.ThrowIfCancellationRequested();
                await _cache.SetAsync(
                    _sessionKey,
                    _store,
                    _idleTimeout,
                    accessingCacheTokenSource.Token);

                _isModified = false;

                _logger.SessionStored(_sessionKey, Id, _store.Count);
            }
            catch (OperationCanceledException operationException)
            {
                if (ioTimeoutTokenSource.Token.IsCancellationRequested)
                {
                    _logger.SessionCommitTimeout();

                    throw new OperationCanceledException(
                        "Timed out committing the session.",
                        operationException,
                        ioTimeoutTokenSource.Token);
                }

                throw;
            }
        }

        private async Task TryLogSessionStartedAsync(CancellationTokenSource cancellationTokenSource)
        {
            try
            {
                cancellationTokenSource.Token.ThrowIfCancellationRequested();

                var storedData = await _cache.GetAsync(_sessionKey, cancellationTokenSource.Token);

                if (storedData == null)
                {
                    _logger.SessionStarted(_sessionKey, Id);
                }
            }
            catch (OperationCanceledException)
            {
                // Ignore this kind of exception.
            }
            catch (Exception exception)
            {
                _logger.SessionCacheReadException(_sessionKey, exception);
            }
        }

        private async Task RefreshAsync(CancellationTokenSource ioTimeoutTokenSource, CancellationTokenSource accessingCacheTokenSource)
        {
            try
            {
                await _cache.RefreshAsync(_sessionKey, accessingCacheTokenSource.Token);
            }
            catch (OperationCanceledException operationException)
            {
                if (ioTimeoutTokenSource.Token.IsCancellationRequested)
                {
                    _logger.SessionRefreshTimeout();

                    throw new OperationCanceledException(
                        "Timed out refreshing the session.",
                        operationException,
                        ioTimeoutTokenSource.Token);
                }

                throw;
            }
        }

        public async Task LoadAsync(CancellationToken cancellationToken = default)
        {
            if (_loaded)
            {
                return;
            }

            using var ioTimeoutTokenSource = new CancellationTokenSource(_ioTimeout);
            var accessingCacheTokenSource = CancellationTokenSource.CreateLinkedTokenSource(ioTimeoutTokenSource.Token, cancellationToken);

            try
            {
                accessingCacheTokenSource.Token.ThrowIfCancellationRequested();

                var newStore = await _cache.GetAsync(_sessionKey, accessingCacheTokenSource.Token);

                if (newStore != null)
                {
                    _store = newStore;
                }
                else
                {
                    if (!_isNewSessionKey)
                    {
                        _logger.AccessingExpiredSession(_sessionKey);
                    }
                }
            }
            catch (OperationCanceledException operationException)
            {
                if (ioTimeoutTokenSource.Token.IsCancellationRequested)
                {
                    _logger.SessionLoadingTimeout();

                    throw new OperationCanceledException(
                        "Timeout when loading the session.",
                        operationException,
                        ioTimeoutTokenSource.Token);
                }

                throw;
            }

            _isAvailable = true;
            _loaded = true;
        }

        private void Load()
        {
            if (_loaded)
            {
                return;
            }

            try
            {
                var newStore = _cache.Get(_sessionKey);

                if (newStore != null)
                {
                    _store = newStore;
                }
                else
                {
                    if (!_isNewSessionKey)
                    {
                        _logger.AccessingExpiredSession(_sessionKey);
                    }
                }

                _isAvailable = true;
            }
            catch (Exception exception)
            {
                _logger.SessionCacheReadException(_sessionKey, exception);

                _isAvailable = false;
                _sessionId = string.Empty;
                _store = new NoOpLocalStore();
            }
            finally
            {
                _loaded = true;
            }
        }

        public void Remove(string key)
        {
            Load();

            _isModified |= _store.Remove(key);
        }

        public void Set(string key, byte[] value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (!IsAvailable)
            {
                return;
            }

            if (!_tryEstablishSession())
            {
                throw new InvalidOperationException("The session cannot be established after the response has started.");
            }

            var clonedValue = new byte[value.Length];

            Buffer.BlockCopy(value, 0, clonedValue, 0, value.Length);

            _store.SetValue(key, clonedValue);
            _isModified = true;
        }

        public bool TryGetValue(string key, out byte[] value)
        {
            Load();

            var (existing, storedValue) = _store.TryGetValue(key);

            value = storedValue;

            return existing;
        }
    }
}