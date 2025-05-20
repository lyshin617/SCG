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
        private readonly Dictionary<string, IRequestHandlerWrapper> _requestHandlers = new();

        private readonly ConcurrentDictionary<string, TaskCompletionSource<ProtocolEnvelope>> _responseAwaiters = new();

        public void RegisterRequestHandler<TRequest, TResponse>(string fullAction, Func<IClientConnection, TRequest, Task<TResponse>> handler)
        {
            _requestHandlers[fullAction] = new RequestHandlerWrapper<TRequest, TResponse>(handler);
        }

        public void RegisterRequestHandler<TRequest>(string fullAction, Func<IClientConnection, TRequest, Task> handler)
        {
            _requestHandlers[fullAction] = new RequestHandlerWrapper<TRequest>(handler);
        }
        internal void RegisterRequestHandler(string fullAction, IRequestHandlerWrapper wrapper)
        {
            _requestHandlers[fullAction] = wrapper;
        }

        public async Task DispatchAsync(IClientConnection conn, ProtocolEnvelope envelope)
        {
            if (envelope.Kind == MessageKind.Request &&
                _requestHandlers.TryGetValue(envelope.MessageType.Action, out var wrapper))
            {
                await wrapper.HandleAsync(conn, envelope, this);
            }
            else if (envelope.Kind == MessageKind.Response &&
                        !string.IsNullOrEmpty(envelope.RequestId) &&
                        _responseAwaiters.TryRemove(envelope.RequestId, out var tcs))
            {
                tcs.SetResult(envelope);
            }
        }

        public async Task<TResponse> SendRequestAsync<TRequest, TResponse>(IClientConnection conn, MessageType messageType, TRequest payload)
        {
            var requestId = Guid.NewGuid().ToString("N");
            var envelope = new ProtocolEnvelope
            {
                Kind = MessageKind.Request,
                MessageType = messageType,
                RequestId = requestId,
                Payload = MemoryPackSerializer.Serialize(payload)
            };

            var tcs = new TaskCompletionSource<ProtocolEnvelope>(TaskCreationOptions.RunContinuationsAsynchronously);
            _responseAwaiters[requestId] = tcs;

            await conn.SendEnvelopeAsync(envelope);
            var responseEnvelope = await tcs.Task;

            return MemoryPackSerializer.Deserialize<TResponse>(responseEnvelope.Payload)!;
        }
        public async Task SendRequestAsync<TRequest>(IClientConnection conn, MessageType messageType, TRequest payload)
        {
            var requestId = Guid.NewGuid().ToString("N");
            var envelope = new ProtocolEnvelope
            {
                Kind = MessageKind.Request,
                MessageType = messageType,
                RequestId = requestId,
                Payload = MemoryPackSerializer.Serialize(payload)
            };

            await conn.SendEnvelopeAsync(envelope);
        }
        public Task BroadcastRequestAsync<T>(IEnumerable<IClientConnection> targets, MessageType messageType, T payload)
        {
            var envelope = new ProtocolEnvelope
            {
                Kind = MessageKind.Request,
                MessageType = messageType,
                RequestId = null, // 不需回應
                Payload = MemoryPackSerializer.Serialize(payload)
            };

            return Task.WhenAll(targets.Select(t => t.SendEnvelopeAsync(envelope)));
        }

    }

}
