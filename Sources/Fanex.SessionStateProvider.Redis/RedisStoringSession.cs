namespace Fanex.SessionStateProvider.Redis
{
    using System.Web.SessionState;

    public class RedisStoringSession
    {
        public RedisStoringSession(
            long? lockId,
            bool locked,
            int? timeoutInSeconds,
            SessionStateActions actions,
            ChangeTrackingSessionStateItemCollection data)
        {
            LockId = lockId;
            Locked = locked;
            TimeoutInSeconds = timeoutInSeconds;
            Actions = actions;
            Data = data;
        }

        public long? LockId { get; }

        public bool Locked { get; }

        public int? TimeoutInSeconds { get; }

        public SessionStateActions Actions { get; }

        public ChangeTrackingSessionStateItemCollection Data { get; }
    }
}