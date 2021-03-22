namespace Fanex.SessionStateProvider.StateServerApi.Controllers
{
    using System.Collections.Generic;
    using System.Web;
    using System.Web.Http;
    using Fanex.SessionStateProvider.StateServerApi.Helpers;
    using Fanex.SessionStateProvider.StateServerApi.Models;

    public class SessionController : ApiController
    {
        [HttpGet]
        public Dictionary<string, byte[]> Get(string id)
            => SessionReflector.GetSession(HttpContext.Current, id);

        [HttpGet]
        public void Refresh(string id)
            => SessionReflector.Refresh(HttpContext.Current, id);

        [HttpPost]
        public void Set(string id, [FromBody] SessionCommit sessionCommit)
            => SessionReflector.SetSession(HttpContext.Current, id, sessionCommit);
    }
}