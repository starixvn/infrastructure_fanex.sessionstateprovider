namespace Fanex.SessionStateProvider.StateServerApi.Models
{
    using System.Collections.Generic;

    public class SessionTransitPayload
    {
        public SessionTransitPayload(string sessionId, Dictionary<string, byte[]> data)
        {
            SessionId = sessionId;
            Data = data;
        }

        public string SessionId { get; }

        public Dictionary<string, byte[]> Data { get; }
    }
}