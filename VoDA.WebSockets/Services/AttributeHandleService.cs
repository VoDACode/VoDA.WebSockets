using Microsoft.AspNetCore.Http;
using System.Reflection;
using VoDA.WebSockets.AttributeHandlers;

namespace VoDA.WebSockets.Services
{
    internal class AttributeHandleService
    {
        public static AttributeHandleService Instance { get; } = new AttributeHandleService();

        private List<BaseHandler> handlers = new List<BaseHandler>();

        private AttributeHandleService()
        {
            LoadHandlers();
        }

        public void HandleAttribute(HttpContext context, object attribute)
        {
            var attributeType = attribute.GetType();
            handlers.FirstOrDefault(p => p.CanHandle(attributeType))?.Handle(context, attribute);
        }

        private void LoadHandlers()
        {
            var targetClasses = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(p => p.Namespace == $"VoDA.WebSockets.AttributeHandlers" && p.BaseType == typeof(BaseHandler));

            foreach (var targetClass in targetClasses)
            {
                var handler = Activator.CreateInstance(targetClass) as BaseHandler;
                if (handler != null)
                {
                    handlers.Add(handler);
                }
            }
        }
    }
}
