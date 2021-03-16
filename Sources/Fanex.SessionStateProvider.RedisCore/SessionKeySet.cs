namespace Fanex.SessionStateProvider.RedisCore
{
    public class SessionKeySet
    {
        private const string DataKeyFormat = "{{{0}_{1}}}_Data";
        private const string InternalKeyFormat = "{{{0}_{1}}}_Internal";

        public SessionKeySet(string applicationName, string sessionKey)
        {
            DataKey = string.Format(DataKeyFormat, applicationName, sessionKey);
            InternalKey = string.Format(InternalKeyFormat, applicationName, sessionKey);
        }

        public string DataKey { get; }

        public string InternalKey { get; }
    }
}