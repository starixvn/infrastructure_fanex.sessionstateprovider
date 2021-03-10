namespace Fanex.SessionStateProvider.Redis.Extensions
{
    using System.Web.SessionState;
    using StackExchange.Redis;

    public static class RedisResultExtensions
    {
        public static RedisStoringSession ParseSession(this RedisResult[] sessionStoredInRedis, ISerializer serializer)
        {
            var lockId = (long?)sessionStoredInRedis[0];
            var rawData = (RedisResult[])sessionStoredInRedis[1];
            var sessionTimeout = (int?)sessionStoredInRedis[2];
            var actions = (SessionStateActions)(int)sessionStoredInRedis[3];
            var locked = (bool)sessionStoredInRedis[4];
            var sessionData = !locked ? DeserializeSessionData(rawData, serializer) : null;

            return new(
                lockId,
                locked,
                sessionTimeout,
                actions,
                sessionData);
        }

        private static ChangeTrackingSessionStateItemCollection DeserializeSessionData(RedisResult[] rawSession, ISerializer serializer)
        {
            const int rawSessionItemSplittingStep = 2;

            var sessionData = new ChangeTrackingSessionStateItemCollection();

            if ((rawSession != null) && (rawSession.Length > 0) && (rawSession.Length % rawSessionItemSplittingStep == 0))
            {
                var counter = 0;

                while (counter + 1 < rawSession.Length)
                {
                    var key = (string)rawSession[counter];
                    var value = rawSession[counter + 1];

                    if (!string.IsNullOrEmpty(key))
                    {
                        sessionData.SetDataWithoutMarkingModifiedKey(key, serializer.Deserialize((byte[])value));
                    }

                    counter += rawSessionItemSplittingStep;
                }
            }

            return sessionData;
        }
    }
}