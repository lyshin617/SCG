using MyGame.GameBackend.App.Core.CustomAttributes;
using System;

namespace MyGame.GameBackend.Modules;
public class Class1
{

}
[RpcModule("Chat")]
public class ChatModule
{
    [Rpc("Chat.OnMessage", RpcKind.Response)]
    public string OnMessage(string req)
    {
        Console.WriteLine($"[Response] ACK received for: {req}");
        return "success";
    }
}

/// [RpcModule("Chat")]
/// public class NotificationHandler
/// {
///     [Rpc("Chat.Notify", RpcKind.Event)]
///     public void HandleNotify(NotifyEvent evt)
///     {
///         Console.WriteLine($"[Event] Notification: {evt.Message}");
///     }
/// }
/// 
