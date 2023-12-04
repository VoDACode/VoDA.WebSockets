using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;
using VoDA.WebSockets.Utilities;

namespace VoDA.WebSockets
{
    public class WebSocketMiddleware
    {
        private Dictionary<RouteAttribute, Type> controllers { get; } = new Dictionary<RouteAttribute, Type>();

        private RequestDelegate next { get; }

        public WebSocketMiddleware(RequestDelegate next)
        {
            this.next = next;
            loadControllers();
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value;
            var matchingRoute = controllers.Keys.FirstOrDefault(route => UriUtilite.IsPathMatch(path, route.Template));

            if (matchingRoute != null)
            {
                if (!context.WebSockets.IsWebSocketRequest)
                {
                    context.Response.StatusCode = StatusCodes.Status405MethodNotAllowed;
                    return;
                }

                var controllerType = controllers[matchingRoute];

                var constructorParameters = controllerType.GetConstructors().First().GetParameters();

                List<object> constructorArguments = new List<object>();
                foreach (var parameter in constructorParameters)
                {
                    var parameterType = parameter.ParameterType;
                    var service = context.RequestServices.GetService(parameterType);
                    if (service is null)
                    {
                        throw new InvalidOperationException($"Unable to resolve service for type {parameterType}.");
                    }
                    constructorArguments.Add(service);
                }

                var controllerInstance = Activator.CreateInstance(controllerType, constructorArguments.ToArray());

                var routeValues = UriUtilite.ExtractRouteValues(path, matchingRoute.Template);

                if (controllerInstance is BaseWebSocketController webSocketController)
                {
                    try
                    {
                        await webSocketController.HandleWebSocket(context, routeValues, matchingRoute);
                    }
                    catch { }
                    return;
                }
            }
            await next(context);
        }

        private void loadControllers()
        {
            var currentNamespace = Assembly.GetEntryAssembly()?.GetName().Name;
            if (currentNamespace is null)
            {
                throw new InvalidOperationException();
            }

            var targetClasses = Assembly.GetEntryAssembly()
                .GetTypes()
                .Where(p => p.Namespace == $"{currentNamespace}.Controllers" && p.BaseType == typeof(BaseWebSocketController));

            foreach (var targetClass in targetClasses)
            {
                RouteAttribute? routeAttribute = targetClass.GetCustomAttribute<RouteAttribute>();
                string className = targetClass.Name;
                string controllerName = className.Replace("WebSocket", "");

                if (routeAttribute is null)
                {
                    routeAttribute = new RouteAttribute($"/{controllerName}/");
                }
                else if (!routeAttribute.Template.StartsWith("/"))
                {
                    routeAttribute = new RouteAttribute($"/{routeAttribute.Template}");
                }


                this.controllers.Add(routeAttribute, targetClass);
            }
        }
    }
}
