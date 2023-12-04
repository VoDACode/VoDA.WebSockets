using System.Net.WebSockets;

namespace VoDA.WebSockets
{
    public class WebSocketCyclicAttribute : Attribute
    {
        public int BufferSize { get; set; } = 1024 * 4;
        public WebSocketMessageType MessageType { get; set; } = WebSocketMessageType.Text;

        public WebSocketCyclicAttribute()
        {
        }

        public WebSocketCyclicAttribute(WebSocketMessageType messageType)
        {
            MessageType = messageType;
        }
    }
}
