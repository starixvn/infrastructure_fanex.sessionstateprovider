namespace Fanex.SessionStateProvider.Redis
{
    using System;
    using System.Threading;
    using System.Web.SessionState;
    using Fanex.SessionStateProvider.Redis.Extensions;
    using StackExchange.Redis;

    public sealed class RedisSessionStorage : IRedisSessionStorage, IDisposable
    {
        private readonly SemaphoreSlim _connectionLock = new(1, 1);
        private readonly string _connectionString;
        private readonly TimeSpan _retryTimeout;
        private readonly ISerializer _serializer;

        private volatile ConnectionMultiplexer _connection;
        private IDatabase _database;

        public RedisSessionStorage(SessionStorageOptions options)
        {
            _connectionString = options.ConnectionString;
            _retryTimeout = options.RetryTimeout;
            _serializer = options.Serializer;
        }

        public void TryUpdateExpiryTime(SessionKeySet keySet, int timeToExpireInSeconds)
            => Eval(RedisCommand.CreateUpdateExpiryTimeCommand(keySet, timeToExpireInSeconds));

        public void Write(
            SessionKeySet keySet,
            ChangeTrackingSessionStateItemCollection sessionData,
            int sessionTimeout,
            SessionStateActions actions)
            => Eval(RedisCommand.CreateWriteDataCommand(
                keySet,
                sessionData,
                sessionTimeout,
                actions,
                _serializer));

        public RedisStoringSession TryObtainLockThenReadData(SessionKeySet keySet, DateTimeOffset lockTime, int lockTimeout)
        {
            var command = RedisCommand.CreateObtainLockThenReadDataCommand(keySet, lockTime, lockTimeout);
            var rawResult = (RedisResult[])Eval(command);

            return rawResult.ParseSession(_serializer);
        }

        public RedisStoringSession TryReadData(SessionKeySet keySet)
        {
            var redisCommand = RedisCommand.CreateReadDataCommand(keySet);
            var rawResult = (RedisResult[])Eval(redisCommand);

            return rawResult.ParseSession(_serializer);
        }

        public void TryReleaseLock(SessionKeySet keySet, object lockId, int sessionTimeout)
            => Eval(RedisCommand.CreateReleaseLockCommand(keySet, lockId, sessionTimeout));

        public void TryDelete(SessionKeySet keySet, object lockId)
            => Eval(RedisCommand.CreateDeleteDataCommand(keySet, lockId));

        public void TryChange(
            SessionKeySet keySet,
            object lockId,
            ChangeTrackingSessionStateItemCollection sessionData,
            int sessionTimeout)
            => Eval(RedisCommand.CreateChangeDataCommand(
                keySet,
                lockId,
                sessionData,
                sessionTimeout,
                _serializer));

        private RedisResult Eval(RedisCommand command)
        {
            if (command == null)
            {
                return default;
            }

            return RetryLogic(() => _database.ScriptEvaluate(command.Script, command.Keys, command.Values));
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
                    _connection = ConnectionMultiplexer.Connect(_connectionString);
                    _database = _connection.GetDatabase();
                }
            }
            finally
            {
                _connectionLock.Release();
            }
        }

        private RedisResult RetryLogic(Func<RedisResult> operation)
        {
            const int timeToSleepFactor = 2;

            var startTime = DateTime.Now;
            var timeToSleepBeforeNextRetryInMiliseconds = 20;

            Connect();

            while (true)
            {
                try
                {
                    return RetryWhenScriptNotFound(operation);
                }
                catch (Exception generallyCatchedException)
                {
                    var passedTime = DateTime.Now - startTime;

                    if (_retryTimeout < passedTime)
                    {
                        throw new RedisSessionStateProviderException("The time for trying to run a command is out.", generallyCatchedException);
                    }

                    var remainingTimeout = (int)(_retryTimeout.TotalMilliseconds - passedTime.TotalMilliseconds);

                    if (remainingTimeout < timeToSleepBeforeNextRetryInMiliseconds)
                    {
                        timeToSleepBeforeNextRetryInMiliseconds = remainingTimeout;
                    }

                    Thread.Sleep(timeToSleepBeforeNextRetryInMiliseconds);
                    timeToSleepBeforeNextRetryInMiliseconds *= timeToSleepFactor;
                }
            }
        }

        private static RedisResult RetryWhenScriptNotFound(Func<RedisResult> operation)
        {
            try
            {
                return operation();
            }
            catch (ObjectDisposedException)
            {
                return operation();
            }
            catch (Exception generallyCatchedException)
            {
                if (generallyCatchedException.Message.Contains("NOSCRIPT"))
                {
                    return operation();
                }

                throw;
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