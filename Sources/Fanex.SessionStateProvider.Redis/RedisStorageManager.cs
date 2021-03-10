namespace Fanex.SessionStateProvider.Redis
{
    using System.Collections.Specialized;

    public static class RedisStorageManager
    {
        private static readonly object OptionsCreationLock = new();
        private static readonly object StorageCreationLock = new();

        public static SessionStorageOptions Options { get; private set; }

        public static IRedisSessionStorage Storage { get; private set; }

        public static void TryInitialize(NameValueCollection config)
        {
            if (Options == null)
            {
                lock (OptionsCreationLock)
                {
                    Options ??= SessionStorageOptions.TryParse(config);
                }
            }

            if (Storage == null)
            {
                lock (StorageCreationLock)
                {
                    Storage ??= new RedisSessionStorage(Options);
                }
            }
        }
    }
}