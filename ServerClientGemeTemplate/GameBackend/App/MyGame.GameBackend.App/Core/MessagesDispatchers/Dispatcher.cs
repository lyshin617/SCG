using MemoryPack;
using MyGame.GameBackend.App.Core.Models;
using MyGame.GameBackend.App.Core.Messages;
using MyGame.GameBackend.App.Core.MessagesDispatchers.Interfaces;
using MyGame.GameBackend.App.Core.Networks.Interfaces;
using System.Collections.Concurrent;

namespace MyGame.GameBackend.App.Core.MessagesDispatchers
{
    public class Dispatcher : IDispatcher
    {
        private readonly Dictionary<string, Func<MessageContext, Task<ProtocolEnvelope>>> _requestHandlers = new();
        private readonly Dictionary<string, Action<MessageContext>> _responseHandlers = new();
        private readonly Dictionary<string, Action<MessageContext>> _eventHandlers = new();


        public void RegisterRequestHandler(string command, Func<MessageContext, Task<ProtocolEnvelope>> handler)
            => _requestHandlers[command] = handler;

        public void RegisterResponseHandler(string command, Action<MessageContext> handler)
            => _responseHandlers[command] = handler;

        public void RegisterEventHandler(string command, Action<MessageContext> handler)
            => _eventHandlers[command] = handler;

        public async Task<DispatchResult> DispatchAsync(MessageContext ctx)
        {
            var envelope = ctx.Envelope;
            var action = envelope.MessageType.Action;

            switch (envelope.Kind)
            {
                case MessageKind.Request:
                    if (_requestHandlers.TryGetValue(action, out var reqHandler))
                    {
                        var response = await reqHandler(ctx);
                        return DispatchResult.Reply(response);
                    }
                    break;

                case MessageKind.Response:
                    if (_responseHandlers.TryGetValue(action, out var respHandler))
                    {
                        respHandler(ctx);
                        return DispatchResult.NoReply(); // response 不需回覆
                    }
                    break;

                case MessageKind.Event:
                    if (_eventHandlers.TryGetValue(action, out var eventHandler))
                    {
                        eventHandler(ctx);
                        return DispatchResult.NoReply();
                    }
                    break;
            }

            return DispatchResult.Drop(); // 若無對應 handler
        }
    }


}
