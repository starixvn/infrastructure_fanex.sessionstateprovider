namespace Fanex.SessionStateProvider.RedisCore
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public interface ICache
    {
        (object LockId, ILocalStore Store) Get(string key);

        Task<(object LockId, ILocalStore Store)> GetAsync(string key, CancellationToken cancellationToken = default);

        Task SetAsync(
            string key,
            object lockId,
            ILocalStore store,
            TimeSpan sessionTimeout,
            CancellationToken cancellationToken = default);

        Task RefreshAsync(string key, CancellationToken cancellationToken = default);
    }
}