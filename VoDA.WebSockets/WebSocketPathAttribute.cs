using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoDA.WebSockets
{
    public class WebSocketPathAttribute : Attribute
    {
        public string Path { get; set; }
        public WebSocketPathAttribute(string path)
        {
            Path = path;
        }
    }
}
