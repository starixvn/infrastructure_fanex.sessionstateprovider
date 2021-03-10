namespace Fanex.SessionStateProvider.Redis
{
    using System;
    using System.Web.SessionState;

    public interface IRedisSessionStorage
    {
        void TryUpdateExpiryTime(SessionKeySet keySet, int timeToExpireInSeconds);

        void Write(
            SessionKeySet keySet,
            ChangeTrackingSessionStateItemCollection sessionData,
            int sessionTimeout,
            SessionStateActions actions);

        RedisStoringSession TryObtainLockThenReadData(SessionKeySet keySet, DateTimeOffset lockTime, int lockTimeout);

        RedisStoringSession TryReadData(SessionKeySet keySet);

        void TryReleaseLock(SessionKeySet keySet, object lockId, int sessionTimeout);

        void TryDelete(SessionKeySet keySet, object lockId);

        void TryChange(
            SessionKeySet keySet,
            object lockId,
            ChangeTrackingSessionStateItemCollection sessionData,
            int sessionTimeout);
    }
}