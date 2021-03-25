namespace Fanex.SessionStateProvider.AspNetCore.Redis
{
    using StackExchange.Redis;

    public static class RedisResultExtensions
    {
        public static (string SessionId, ILocalStore Store) TryParseToLocalStore(this RedisResult[] rawRedisData)
        {
            const int rawSessionItemSplittingStep = 2;
            const int expectedRawDataLength = 2;

            if (rawRedisData == null || rawRedisData.Length != expectedRawDataLength)
            {
                return default;
            }

            var sessionId = (string)rawRedisData[0];
            var rawSession = (RedisResult[])rawRedisData[1];

            if ((rawSession == null) || (rawSession.Length == 0) || (rawSession.Length % rawSessionItemSplittingStep != 0))
            {
                return default;
            }

            var sessionStore = new LocalStore();
            var counter = 0;

            while (counter + 1 < rawSession.Length)
            {
                var key = (string)rawSession[counter];
                var value = rawSession[counter + 1];

                if (!string.IsNullOrEmpty(key))
                {
                    sessionStore.SetValueWithoutMarkingModified(key, (byte[])value);
                }

                counter += rawSessionItemSplittingStep;
            }

            return (sessionId, sessionStore);
        }
    }
}