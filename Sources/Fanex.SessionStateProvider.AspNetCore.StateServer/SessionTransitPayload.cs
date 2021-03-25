namespace Fanex.SessionStateProvider.AspNetCore.StateServer
{
    using System.Collections.Generic;

    public class SessionTransitPayload
    {
        public string SessionId { get; set; }

        public Dictionary<string, byte[]> Data { get; set; }
    }
}