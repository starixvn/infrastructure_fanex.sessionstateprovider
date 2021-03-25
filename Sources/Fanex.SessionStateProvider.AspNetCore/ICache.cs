namespace Fanex.SessionStateProvider.AspNetCore
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public interface ICache
    {
        (string SessionId, ILocalStore Store) Get(string key);

        Task<(string SessionId, ILocalStore Store)> GetAsync(string key, CancellationToken cancellationToken = default);

        Task SetAsync(
            string key,
            string sessionId,
            ILocalStore store,
            TimeSpan sessionTimeout,
            CancellationToken cancellationToken = default);

        Task RefreshAsync(string key, CancellationToken cancellationToken = default);
    }
}