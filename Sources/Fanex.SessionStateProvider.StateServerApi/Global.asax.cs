namespace Fanex.SessionStateProvider.StateServerApi
{
    using System.Web;
    using System.Web.Http;

#pragma warning disable SA1649 // File name should match first type name

    public class WebApiApplication : HttpApplication
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);
        }
    }
}