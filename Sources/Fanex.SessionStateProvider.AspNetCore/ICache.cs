namespace Fanex.SessionStateProvider.AspNetCore
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public interface ICache
    {
        ILocalStore Get(string key);

        Task<ILocalStore> GetAsync(string key, CancellationToken cancellationToken = default);

        Task SetAsync(
            string key,
            ILocalStore store,
            TimeSpan sessionTimeout,
            CancellationToken cancellationToken = default);

        Task RefreshAsync(string key, CancellationToken cancellationToken = default);
    }
}