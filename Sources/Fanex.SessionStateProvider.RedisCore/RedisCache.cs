namespace Fanex.SessionStateProvider.RedisCore
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Fanex.SessionStateProvider.RedisCore.Extensions;
    using Microsoft.Extensions.Options;
    using StackExchange.Redis;

    public sealed class RedisCache : ICache, IDisposable
    {
        private readonly SemaphoreSlim _connectionLock = new(1, 1);
        private readonly RedisCacheOptions _options;

        private volatile ConnectionMultiplexer _connection;
        private IDatabase _database;

        public RedisCache(IOptions<RedisCacheOptions> options)
            => _options = options?.Value ?? throw new ArgumentNullException(nameof(options));

        public ILocalStore Get(string key)
        {
            var command = RedisCommand.CreateGetCommand(new SessionKeySet(_options.ApplicationName, key));
            var rawResult = (RedisResult[])Eval(command);

            return rawResult.TryParseToLocalStore();
        }

        public async Task<ILocalStore> GetAsync(string key, CancellationToken cancellationToken = default)
        {
            var command = RedisCommand.CreateGetCommand(new SessionKeySet(_options.ApplicationName, key));
            var rawResult = (RedisResult[])await EvalAsync(command, cancellationToken).ConfigureAwait(false);

            return rawResult.TryParseToLocalStore();
        }

        public Task RefreshAsync(string key, CancellationToken cancellationToken = default)
            => EvalAsync(
                RedisCommand.CreateRefreshSessionTimeoutCommand(new SessionKeySet(_options.ApplicationName, key)),
                cancellationToken);

        public Task SetAsync(
            string key,
            ILocalStore store,
            TimeSpan sessionTimeout,
            CancellationToken cancellationToken = default)
            => EvalAsync(
                RedisCommand.CreateSetCommand(new SessionKeySet(_options.ApplicationName, key), store, sessionTimeout.Seconds),
                cancellationToken);

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

            cancellationToken.ThrowIfCancellationRequested();
            await ConnectAsync(cancellationToken).ConfigureAwait(false);

            return await _database.ScriptEvaluateAsync(command.Script, command.Keys, command.Values).ConfigureAwait(false);
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

            cancellationToken.ThrowIfCancellationRequested();
            await _connectionLock.WaitAsync(cancellationToken).ConfigureAwait(false);

            try
            {
                if (_database == null)
                {
                    _connection = await ConnectionMultiplexer.ConnectAsync(_options.ConnectionString).ConfigureAwait(false);
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