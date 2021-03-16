namespace Fanex.SessionStateProvider.Redis
{
    using System.Collections.Specialized;

    public static class RedisStorageManager
    {
        private static readonly object _optionsCreationLock = new();
        private static readonly object _storageCreationLock = new();

        public static SessionStorageOptions Options { get; private set; }

        public static IRedisSessionStorage Storage { get; private set; }

        public static void TryInitialize(NameValueCollection config)
        {
            if (Options == null)
            {
                lock (_optionsCreationLock)
                {
                    Options ??= SessionStorageOptions.TryParse(config);
                }
            }

            if (Storage == null)
            {
                lock (_storageCreationLock)
                {
                    Storage ??= new RedisSessionStorage(Options);
                }
            }
        }
    }
}