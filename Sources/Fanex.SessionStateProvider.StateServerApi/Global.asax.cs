namespace Fanex.SessionStateProvider.StateServerApi
{
    using System;
    using System.Web;
    using System.Web.Http;
    using Fanex.Logging.Extensions;
    using Fanex.Logging.Extensions.Web;

#pragma warning disable SA1649 // File name should match first type name

    public class WebApiApplication : HttpApplication
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);
            ExceptionHandler.UseConfig(new LoggingConfiguration().GetHttpHeaderInfo());
        }

        protected void Application_Error(object sender, EventArgs e)
            => ExceptionHandler.Handle(Server.GetLastError());
    }
}