namespace Fanex.SessionStateProvider.Redis
{
    using System;
    using System.Collections.Specialized;
    using System.Web;
    using System.Web.SessionState;

    public class RedisSessionStateProvider : SessionStateStoreProviderBase
    {
        private const int FromMinuteToSeconds = 60;

#pragma warning disable S3962 // "static readonly" constants should be "const" instead
        private static readonly string LastExceptionId = $"{nameof(RedisSessionStateProvider)}_LastException";
#pragma warning restore S3962 // "static readonly" constants should be "const" instead

        private SessionStorageOptions _options;
        private IRedisSessionStorage _storage;
        private string _sessionId;
        private object _lockId;
        private SessionKeySet _sessionKeySet;

        public static Exception LastException
        {
            get => HttpContext.Current != null ? HttpContext.Current.Items[LastExceptionId] as Exception : null;

            set
            {
                if (HttpContext.Current != null)
                {
                    HttpContext.Current.Items[LastExceptionId] = value;
                }
            }
        }

        public override void Initialize(string name, NameValueCollection config)
        {
            base.Initialize(name, config);
            RedisStorageManager.TryInitialize(config);

            _options = RedisStorageManager.Options;
            _storage = RedisStorageManager.Storage;
        }

        public override SessionStateStoreData CreateNewStoreData(HttpContext context, int timeout)
            => new(new ChangeTrackingSessionStateItemCollection(), new HttpStaticObjectsCollection(), timeout);

        public override void CreateUninitializedItem(HttpContext context, string id, int timeout)
        {
            try
            {
                if (LastException == null)
                {
                    var sessionData = new ChangeTrackingSessionStateItemCollection();
                    var sessionTimeoutInSeconds = timeout * FromMinuteToSeconds;

                    RefreshSessionKeySet(id);
                    _storage.Write(
                        _sessionKeySet,
                        sessionData,
                        sessionTimeoutInSeconds,
                        SessionStateActions.InitializeItem);
                }
            }
            catch (Exception exception)
            {
                LastException = exception;
                throw;
            }
        }

        public override void Dispose()
        {
        }

        public override void EndRequest(HttpContext context)
        {
            ReleaseLock(context, _sessionId, _lockId);
            _sessionKeySet = null;
        }

        public override SessionStateStoreData GetItem(
            HttpContext context,
            string id,
            out bool locked,
            out TimeSpan lockAge,
            out object lockId,
            out SessionStateActions actions)
            => GetItem(
                false,
                context,
                id,
                out locked,
                out lockAge,
                out lockId,
                out actions);

        public override SessionStateStoreData GetItemExclusive(
            HttpContext context,
            string id,
            out bool locked,
            out TimeSpan lockAge,
            out object lockId,
            out SessionStateActions actions)
            => GetItem(
                true,
                context,
                id,
                out locked,
                out lockAge,
                out lockId,
                out actions);

        private SessionStateStoreData GetItem(
            bool lockRequired,
            HttpContext context,
            string id,
            out bool locked,
            out TimeSpan lockAge,
            out object lockId,
            out SessionStateActions actions)
        {
            if (id == null)
            {
                locked = false;
                lockAge = TimeSpan.Zero;
                lockId = 0;
                actions = SessionStateActions.None;

                return default;
            }

            try
            {
                RefreshSessionKeySet(id);

                var storingSessionData = lockRequired
                    ? _storage.TryObtainLockThenReadData(_sessionKeySet, DateTimeOffset.Now, (int)_options.RequestTimeout.TotalSeconds)
                    : _storage.TryReadData(_sessionKeySet);
                var sessionTimeoutInMinutes = storingSessionData.TimeoutInSeconds.HasValue
                    ? storingSessionData.TimeoutInSeconds.Value / FromMinuteToSeconds
                    : (int)_options.SessionTimeout.TotalMinutes;

                if (lockRequired)
                {
                    _sessionId = id;
                    _lockId = storingSessionData.LockId;
                }

                locked = storingSessionData.Locked;
                lockId = storingSessionData.LockId;
                actions = SessionStateActions.None;

                if (locked)
                {
                    lockAge = GetLockAge(storingSessionData.LockId);
                    _sessionId = null;
                    _lockId = null;
                }
                else
                {
                    lockAge = TimeSpan.Zero;

                    if (storingSessionData.Data == null)
                    {
                        ReleaseItemExclusive(context, id, lockId);
                    }
                    else
                    {
                        actions = storingSessionData.Actions;
                        storingSessionData.Data.Dirty = false;

                        return new(storingSessionData.Data, new HttpStaticObjectsCollection(), sessionTimeoutInMinutes);
                    }
                }

                return default;
            }
            catch (Exception exception)
            {
                LastException = exception;
                throw;
            }
        }

        private static TimeSpan GetLockAge(object lockId)
        {
            try
            {
                var lockTimeInTicks = (long)lockId;

                return DateTimeOffset.UtcNow.Subtract(new DateTimeOffset(lockTimeInTicks, TimeSpan.Zero));
            }
            catch
            {
                return DateTimeOffset.UtcNow.Subtract(default(DateTimeOffset));
            }
        }

        public override void InitializeRequest(HttpContext context)
        {
        }

        public override void ReleaseItemExclusive(HttpContext context, string id, object lockId)
        {
            if (LastException != null || lockId == null)
            {
                return;
            }

            ReleaseLock(context, id, lockId);
        }

        private void ReleaseLock(HttpContext context, string sessionId, object lockId)
        {
            try
            {
                var sessionTimeout = (context?.Session?.Timeout * FromMinuteToSeconds) ?? (int)_options.SessionTimeout.TotalSeconds;

                RefreshSessionKeySet(sessionId);
                _storage.TryReleaseLock(_sessionKeySet, lockId, sessionTimeout);

                _sessionId = null;
                _lockId = null;
            }
            catch (Exception exception)
            {
                LastException = exception;
                throw;
            }
        }

        public override void RemoveItem(
            HttpContext context,
            string id,
            object lockId,
            SessionStateStoreData item)
        {
            try
            {
                if (LastException != null || lockId == null)
                {
                    return;
                }

                RefreshSessionKeySet(id);
                _storage.TryDelete(_sessionKeySet, lockId);
            }
            catch (Exception exception)
            {
                LastException = exception;
                throw;
            }
        }

        public override void ResetItemTimeout(HttpContext context, string id)
        {
            try
            {
                if (LastException != null)
                {
                    return;
                }

                RefreshSessionKeySet(id);
                _storage.TryUpdateExpiryTime(_sessionKeySet, (int)_options.SessionTimeout.TotalSeconds);
            }
            catch (Exception exception)
            {
                LastException = exception;
                throw;
            }
        }

        public override void SetAndReleaseItemExclusive(
            HttpContext context,
            string id,
            SessionStateStoreData item,
            object lockId,
            bool newItem)
        {
            try
            {
                if (LastException != null)
                {
                    return;
                }

                if (newItem)
                {
                    WriteNewSession(id, item);
                }
                else
                {
                    TryUpdateSession(id, lockId, item);
                }
            }
            catch (Exception exception)
            {
                LastException = exception;
                throw;
            }
        }

        private void WriteNewSession(string sessionId, SessionStateStoreData sessionData)
        {
            if (sessionData == null)
            {
                return;
            }

            var sessionTimeoutInSeconds = sessionData.Timeout * FromMinuteToSeconds;
            var castedSessionData = (sessionData.Items as ChangeTrackingSessionStateItemCollection) ?? new ChangeTrackingSessionStateItemCollection();

            RefreshSessionKeySet(sessionId);
            _storage.Write(_sessionKeySet, castedSessionData, sessionTimeoutInSeconds, SessionStateActions.None);
        }

        private void TryUpdateSession(string sessionId, object lockId, SessionStateStoreData sessionData)
        {
            if (sessionData == null)
            {
                return;
            }

            var sessionTimeoutInSeconds = sessionData.Timeout * FromMinuteToSeconds;
            var castedSessionData = sessionData.Items as ChangeTrackingSessionStateItemCollection;

            RefreshSessionKeySet(sessionId);
            _storage.TryChange(_sessionKeySet, lockId, castedSessionData, sessionTimeoutInSeconds);
        }

        public override bool SetItemExpireCallback(SessionStateItemExpireCallback expireCallback)
            => false;

        private void RefreshSessionKeySet(string sessionId)
        {
            if (_sessionKeySet == null)
            {
                _sessionKeySet = new SessionKeySet(_options.ApplicationName, sessionId);
            }
            else
            {
                _sessionKeySet.Refresh(sessionId);
            }
        }
    }
}