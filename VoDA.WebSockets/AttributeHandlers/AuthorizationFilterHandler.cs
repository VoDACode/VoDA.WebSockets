using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;

namespace VoDA.WebSockets.AttributeHandlers
{
    internal class AuthorizationFilterHandler : BaseHandler
    {
        public override Type TargetType => typeof(IAuthorizationFilter);

        public override void Handle(HttpContext context, object instance)
        {
            var filter = instance as IAuthorizationFilter;
            if (filter != null)
            {
                var actionContext = new ActionContext(context, new RouteData(), new ActionDescriptor());
                var authorizationFilterContext = new AuthorizationFilterContext(actionContext, new List<IFilterMetadata>());
                filter.OnAuthorization(authorizationFilterContext);
                if(authorizationFilterContext.Result != null)
                {
                    context.Response.StatusCode = (authorizationFilterContext.Result as StatusCodeResult)?.StatusCode ?? 403;
                    context.Response.WriteAsync(authorizationFilterContext.Result.ToString() ?? "Forbidden");
                    context.Response.Body.Close();
                }
            }
        }
    }
}
