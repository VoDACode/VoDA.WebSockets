using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net.WebSockets;
using System.Reflection;
using VoDA.WebSockets.Exceptions;
using VoDA.WebSockets.Services;
using VoDA.WebSockets.Utilities;

namespace VoDA.WebSockets
{
    public abstract class BaseWebSocketController
    {
        protected HttpContext HttpContext => httpContext;
        protected WebSocketClient Client => webSocketClient;

        private HttpContext httpContext { get; set; } = null!;
        private WebSocketClient webSocketClient { get; set; } = null!;

        public async Task HandleWebSocket(HttpContext context, Dictionary<string, string> routeValues, RouteAttribute controllerTemplate)
        {
            httpContext = context;

            IEnumerable<MethodInfo> methods = GetType().GetMethods().Where(m => !m.IsPrivate);
            var pathWithoutControllerTemplate = UriUtilite.GetPathToFunction(context.Request.Path.Value, controllerTemplate.Template);

            try
            {
                foreach (MethodInfo method in methods)
                {
                    string methodPath = GetMethodPath(method);
                    if (UriUtilite.IsPathMatch(pathWithoutControllerTemplate, methodPath))
                    {
                        await HandleMethod(context, method, pathWithoutControllerTemplate, methodPath, routeValues);
                        return;
                    }
                }
            }
            catch (SuccessException) { }
            catch (IOException) { }
            catch (FailureException ex)
            {
                Console.WriteLine(ex.ToString());
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                await ClientManager.Instance.Remove(webSocketClient);
            }
        }

        private async Task HandleMethod(HttpContext context, MethodInfo method, string pathWithoutControllerTemplate, string methodPath, Dictionary<string, string> routeValues)
        {
            var functionRouterParameters = UriUtilite.ExtractRouteValues(pathWithoutControllerTemplate, methodPath);
            var cyclicAttribute = method.GetCustomAttribute<WebSocketCyclicAttribute>();

            foreach (var atrebute in method.GetCustomAttributes())
            {
                AttributeHandleService.Instance.HandleAttribute(context, atrebute);
                if (context.Response.HasStarted)
                {
                    return;
                }
            }

            foreach (var item in functionRouterParameters)
            {
                routeValues[item.Key] = item.Value;
            }

            var methodParameters = await GetWebSocketParameners(context, method, routeValues);

            var socket = await context.WebSockets.AcceptWebSocketAsync();
            webSocketClient = ClientManager.Instance.Add(context, socket);
            if (cyclicAttribute is not null)
            {
                await Client.SendAsync("Connected");
                methodParameters.Add(cyclicAttribute.MessageType == WebSocketMessageType.Text ? string.Empty : new byte[0]);
                while (Client.Socket.State != WebSocketState.Closed)
                {
                    methodParameters.RemoveAt(methodParameters.Count - 1);
                    methodParameters.Add(cyclicAttribute.MessageType == WebSocketMessageType.Text ? await Client.ReceiveAsync(cyclicAttribute.BufferSize) : await Client.ReceiveBytesAsync(cyclicAttribute.BufferSize));
                    method.Invoke(this, methodParameters.ToArray());
                }
            }
            else
            {
                method.Invoke(this, methodParameters.ToArray());
            }
        }

        private async Task<List<object>> GetWebSocketParameners(HttpContext context, MethodInfo method, Dictionary<string, string> routeValues)
        {
            var methodParameters = new List<object>();
            var parameters = method.GetParameters();

            var cyclicAttribute = method.GetCustomAttribute<WebSocketCyclicAttribute>();
            for (int i = 0; i < parameters.Length - (cyclicAttribute is null ? 0 : 1); i++)
            {
                var parameter = parameters[i];
                if (routeValues.TryGetValue(parameter.Name, out var value))
                {
                    var convertedValue = Convert.ChangeType(value, parameter.ParameterType);
                    methodParameters.Add(convertedValue);
                }
                else
                {
                    var queryValue = context.Request.Query[parameter.Name].FirstOrDefault();
                    if (queryValue != null)
                    {
                        var convertedQueryValue = Convert.ChangeType(queryValue, parameter.ParameterType);
                        methodParameters.Add(convertedQueryValue);
                    }
                    else
                    {
                        if (!parameter.HasDefaultValue)
                        {
                            context.Response.StatusCode = StatusCodes.Status415UnsupportedMediaType;
                            await context.Response.WriteAsync("Unsupported media type");
                            throw new NotSupportedMediaTypeException();
                        }

                        methodParameters.Add(parameter.DefaultValue);
                    }
                }
            }

            return methodParameters;
        }

        private string GetMethodPath(MethodInfo method)
        {
            WebSocketPathAttribute? webSocketPath = method.GetCustomAttribute<WebSocketPathAttribute>();
            return webSocketPath?.Path ?? method.Name;
        }
    }
}
