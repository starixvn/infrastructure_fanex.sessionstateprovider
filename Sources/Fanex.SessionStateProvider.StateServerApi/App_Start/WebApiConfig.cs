namespace Fanex.SessionStateProvider.StateServerApi
{
    using System.Web.Http;
    using Fanex.SessionStateProvider.StateServerApi.Filters;
    using Fanex.SessionStateProvider.StateServerApi.Formatters;

    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.Filters.Add(new GlobalExceptionFilterAttribute());
            config.Formatters.Add(new ContractlessMessagePackFormatter());
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{action}/{id}",
                defaults: new { controller = "Session" });
        }
    }
}