namespace Fanex.SessionStateProvider.AspNetCore.StateServer
{
    using System;
    using Microsoft.Extensions.Options;

    public class StateServerCacheOptions : IOptions<StateServerCacheOptions>
    {
        public Uri ServiceUri { get; set; }

        StateServerCacheOptions IOptions<StateServerCacheOptions>.Value => this;
    }
}