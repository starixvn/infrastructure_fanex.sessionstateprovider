namespace Fanex.SessionStateProvider.StateServerApi.Models
{
    using System;
    using System.Collections.Generic;

    public class SessionCommit
    {
        public Dictionary<string, byte[]> ModifiedItems { get; set; }

        public List<string> DeletedKeys { get; set; }

        public TimeSpan SessionTimeout { get; set; }
    }
}