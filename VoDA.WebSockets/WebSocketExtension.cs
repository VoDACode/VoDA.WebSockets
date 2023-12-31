using Microsoft.AspNetCore.Builder;

namespace VoDA.WebSockets
{
    public static class WebSocketExtension
    {
        public static IApplicationBuilder UseVoDAWebSocket(this IApplicationBuilder applicationBuilder)
        {
            applicationBuilder.UseWebSockets(new WebSocketOptions()
            {
                KeepAliveInterval = TimeSpan.FromSeconds(5)
            });
            applicationBuilder.UseMiddleware<WebSocketMiddleware>();
            return applicationBuilder;
        }
    }
}