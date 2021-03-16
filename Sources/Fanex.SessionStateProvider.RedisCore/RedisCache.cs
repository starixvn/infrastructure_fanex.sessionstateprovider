namespace Fanex.SessionStateProvider.RedisCore
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Fanex.SessionStateProvider.RedisCore.Extensions;
    using StackExchange.Redis;

    public sealed class RedisCache : ICache, IDisposable
    {
        private readonly SemaphoreSlim _connectionLock = new(1, 1);
        private readonly RedisCacheOptions _options;

        private volatile ConnectionMultiplexer _connection;
        private IDatabase _database;

        public RedisCache(RedisCacheOptions options)
            => _options = options ?? throw new ArgumentNullException(nameof(options));

        public (object LockId, ILocalStore Store) Get(string key)
        {
            var command = RedisCommand.CreateGetCommand(new SessionKeySet(_options.ApplicationName, key));
            var rawResult = (RedisResult[])Eval(command);

            return rawResult.TryParseToLocalStore();
        }

        public async Task<(object LockId, ILocalStore Store)> GetAsync(string key, CancellationToken cancellationToken = default)
        {
            var command = RedisCommand.CreateGetCommand(new SessionKeySet(_options.ApplicationName, key));
            var rawResult = (RedisResult[])await EvalAsync(command, cancellationToken);

            return rawResult.TryParseToLocalStore();
        }

        public Task RefreshAsync(string key, CancellationToken cancellationToken = default)
            => EvalAsync(
                RedisCommand.CreateRefreshSessionTimeoutCommand(new SessionKeySet(_options.ApplicationName, key)),
                cancellationToken);

        public async Task SetAsync(
            string key,
            object lockId,
            ILocalStore store,
            TimeSpan sessionTimeout,
            CancellationToken cancellationToken = default)
        {
            const int timeToSleepFactor = 2;

            var command = RedisCommand.CreateSetCommand(
                new SessionKeySet(_options.ApplicationName, key),
                lockId,
                store,
                sessionTimeout.Seconds);
            var startTime = DateTimeOffset.UtcNow;
            var timeToSleepBeforeNextRetryInMiliseconds = 20;

            while (true)
            {
                var successfullySyncData = (bool)await EvalAsync(command, cancellationToken);
                var passedTime = DateTimeOffset.UtcNow - startTime;

                if (successfullySyncData || passedTime > _options.RetryTimeout)
                {
                    return;
                }

                var remainingTimeout = (int)(_options.RetryTimeout.TotalMilliseconds - passedTime.TotalMilliseconds);

                if (remainingTimeout < timeToSleepBeforeNextRetryInMiliseconds)
                {
                    timeToSleepBeforeNextRetryInMiliseconds = remainingTimeout;
                }

                await Task.Delay(timeToSleepBeforeNextRetryInMiliseconds);
                timeToSleepBeforeNextRetryInMiliseconds *= timeToSleepFactor;
            }
        }

        private RedisResult Eval(RedisCommand command)
        {
            if (command == null)
            {
                return default;
            }

            Connect();

            return _database.ScriptEvaluate(command.Script, command.Keys, command.Values);
        }

        private async Task<RedisResult> EvalAsync(RedisCommand command, CancellationToken cancellationToken)
        {
            if (command == null)
            {
                return default;
            }

            await ConnectAsync(cancellationToken);

            return await _database.ScriptEvaluateAsync(command.Script, command.Keys, command.Values);
        }

        private void Connect()
        {
            if (_database != null)
            {
                return;
            }

            _connectionLock.Wait();

            try
            {
                if (_database == null)
                {
                    _connection = ConnectionMultiplexer.Connect(_options.ConnectionString);
                    _database = _connection.GetDatabase();
                }
            }
            finally
            {
                _connectionLock.Release();
            }
        }

        private async Task ConnectAsync(CancellationToken cancellationToken)
        {
            if (_database != null)
            {
                return;
            }

            await _connectionLock.WaitAsync(cancellationToken);

            try
            {
                if (_database == null)
                {
                    _connection = await ConnectionMultiplexer.ConnectAsync(_options.ConnectionString);
                    _database = _connection.GetDatabase();
                }
            }
            finally
            {
                _connectionLock.Release();
            }
        }

        public void Dispose()
        {
            _connectionLock.Dispose();
            _connection?.Dispose();
            _connection = null;
        }
    }
}