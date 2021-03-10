namespace Fanex.SessionStateProvider.Redis
{
    public class SessionKeySet
    {
        private const string DataKeyFormat = "{{{0}_{1}}}_Data";
        private const string LockKeyFormat = "{{{0}_{1}}}_WriteLock";
        private const string InternalKeyFormat = "{{{0}_{1}}}_Internal";

        private readonly string _applicationName;

        private string _sessionId;

        public SessionKeySet(string applicationName, string sessionId)
        {
            _applicationName = applicationName;
            _sessionId = sessionId;

            DataKey = string.Format(DataKeyFormat, _applicationName, _sessionId);
            LockKey = string.Format(LockKeyFormat, _applicationName, _sessionId);
            InternalKey = string.Format(InternalKeyFormat, _applicationName, _sessionId);
        }

        public string DataKey { get; private set; }

        public string LockKey { get; private set; }

        public string InternalKey { get; private set; }

        public void Refresh(string sessionId)
        {
            if (_sessionId != sessionId)
            {
                _sessionId = sessionId;

                DataKey = string.Format(DataKeyFormat, _applicationName, _sessionId);
                LockKey = string.Format(LockKeyFormat, _applicationName, _sessionId);
                InternalKey = string.Format(InternalKeyFormat, _applicationName, _sessionId);
            }
        }
    }
}