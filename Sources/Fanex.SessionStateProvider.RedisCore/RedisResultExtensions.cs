namespace Fanex.SessionStateProvider.RedisCore.Extensions
{
    using StackExchange.Redis;

    public static class RedisResultExtensions
    {
        public static (object LockId, ILocalStore Store) TryParseToLocalStore(this RedisResult[] sessionStoredInRedis)
        {
            var lockId = (long?)sessionStoredInRedis[0];
            var rawData = (RedisResult[])sessionStoredInRedis[1];
            var sessionData = DeserializeSessionData(rawData);

            return (lockId, sessionData);
        }

        private static ILocalStore DeserializeSessionData(RedisResult[] rawSession)
        {
            const int rawSessionItemSplittingStep = 2;

            var sessionData = new LocalStore();

            if ((rawSession != null) && (rawSession.Length > 0) && (rawSession.Length % rawSessionItemSplittingStep == 0))
            {
                var counter = 0;

                while (counter + 1 < rawSession.Length)
                {
                    var key = (string)rawSession[counter];
                    var value = rawSession[counter + 1];

                    if (!string.IsNullOrEmpty(key))
                    {
                        sessionData.SetValueWithoutMarkingModified(key, (byte[])value);
                    }

                    counter += rawSessionItemSplittingStep;
                }
            }

            return sessionData;
        }
    }
}