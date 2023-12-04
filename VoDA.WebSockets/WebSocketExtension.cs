using Microsoft.AspNetCore.Builder;

namespace VoDA.WebSockets
{
    public static class WebSocketExtension
    {
        public static IApplicationBuilder UseVoDAWebSocket(this IApplicationBuilder applicationBuilder)
        {
            applicationBuilder.UseWebSockets();
            applicationBuilder.UseMiddleware<WebSocketMiddleware>();
            return applicationBuilder;
        }
    }
}