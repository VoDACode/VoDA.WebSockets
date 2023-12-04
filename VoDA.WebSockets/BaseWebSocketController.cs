using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net.WebSockets;
using System.Reflection;
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

            // Get all methods from the class
            IEnumerable<MethodInfo> methods = GetType().GetMethods().Where(m => !m.IsPrivate);

            var requestPathWhioutControllerTemplate = UriUtilite.GetPathToFunction(context.Request.Path.Value, controllerTemplate.Template);

            foreach (MethodInfo method in methods)
            {
                WebSocketPathAttribute? webSocketPath = method.GetCustomAttribute<WebSocketPathAttribute>();
                string path = webSocketPath?.Path ?? method.Name;

                // Check if the path matches the template
                if (UriUtilite.IsPathMatch(requestPathWhioutControllerTemplate, path))
                {
                    var functionRouterParameters = UriUtilite.ExtractRouteValues(requestPathWhioutControllerTemplate, path);
                    var cyclicAttribute = method.GetCustomAttribute<WebSocketCyclicAttribute>();

                    foreach (var item in functionRouterParameters)
                    {
                        routeValues[item.Key] = item.Value;
                    }

                    // Invoke the method with appropriate parameters
                    var parameters = method.GetParameters();
                    var methodParameters = new List<object>();

                    for (int i = 0; i < parameters.Length - (cyclicAttribute is null ? 0 : 1); i++)
                    {
                        var parameter = parameters[i];
                        if (routeValues.TryGetValue(parameter.Name, out var value))
                        {
                            // Convert the string value to the parameter type
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

                                // if the parameter is not found and parameter isn`t optional, return media type not supported
                                if (!parameter.HasDefaultValue)
                                {
                                    context.Response.StatusCode = StatusCodes.Status415UnsupportedMediaType;
                                    await context.Response.WriteAsync("Unsupported media type");
                                    return;
                                }

                                methodParameters.Add(parameter.DefaultValue);
                            }
                        }
                    }

                    var socket = await context.WebSockets.AcceptWebSocketAsync();
                    webSocketClient = ClientManager.Instance.Add(context, socket);

                    try
                    {
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
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }
                    await ClientManager.Instance.Remove(webSocketClient);
                    return;
                }
            }
        }
    }
}
