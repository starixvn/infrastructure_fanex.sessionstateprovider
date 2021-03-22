namespace Fanex.SessionStateProvider.StateServerApi.Filters
{
    using System;
    using System.Web.Http.Filters;
    using Fanex.Logging.Extensions.Web;

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    public class GlobalExceptionFilterAttribute : ExceptionFilterAttribute
    {
        public override void OnException(HttpActionExecutedContext actionExecutedContext)
        {
            base.OnException(actionExecutedContext);

            ExceptionHandler.Handle(actionExecutedContext.Exception);
        }
    }
}