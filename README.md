[![Stand With Ukraine](https://raw.githubusercontent.com/VoDACode/VoDA.WebSockets/master/docs/img/banner2-direct.svg)](https://vshymanskyy.github.io/StandWithUkraine/)

[![nuget](https://img.shields.io/static/v1?label=NuGet&message=VoDA.WebSockets&color=blue&logo=nuget)](https://www.nuget.org/packages/VoDA.WebSockets)

# Description

This library provides an easy way to use WebSockets in your ASP.NET project. The usage method is similar to ASP.NET MVC controllers, but for WebSocket.

# Quick Start

- Install the [NuGet package](https://www.nuget.org/packages/VoDA.WebSockets) into your project.

- Create a new class that inherits from `BaseWebSocketController`.

  - You can add a [RouteAttribute](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.routeattribute?view=aspnetcore-7.0) attribute to your class for custom routing.

- Add methods to your class that will handle WebSocket events.

  - You can add a [WebSocketPathAttribute](/VoDA.WebSockets/WebSocketPathAttribute.cs) attribute to your method for custom routing.

  - You can add a [WebSocketCyclicAttribute](/VoDA.WebSockets/WebSocketCyclicAttribute.cs) attribute to your method to make it cyclic (see [example](README.md#example)).

# Example

```csharp
[Route("/ws")]
public class MyWebSocketController : BaseWebSocketController
{
    [WebSocketPath("/")]
    public async Task OnOpenAsync()
    {
        await Client.SendAsync("Hello!");
    }

    [WebSocketCyclic]
    [WebSocketPath("echo")]
    public async Task OnMessageAsync(string message)
    {
        await Client.SendAsync(message);
    }

    [WebSocketPath("{id}")]
    public async Task OnCyclicAsync(string id)
    {
        await Client.SendAsync("Hello " + id);
        await Client.SendAsync("Bye " + id);
    }
}
```
