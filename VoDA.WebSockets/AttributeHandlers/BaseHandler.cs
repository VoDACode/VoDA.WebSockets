using Microsoft.AspNetCore.Http;

namespace VoDA.WebSockets.AttributeHandlers
{
    internal abstract class BaseHandler
    {
        public abstract Type TargetType { get; }

        public abstract void Handle(HttpContext context, object instance);

        public bool CanHandle(Type type)
        {
            return type.IsAssignableTo(TargetType);
        }
    }
}
