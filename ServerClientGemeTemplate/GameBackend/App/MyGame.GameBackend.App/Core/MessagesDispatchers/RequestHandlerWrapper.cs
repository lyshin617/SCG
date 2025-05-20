using MemoryPack;
using MyGame.GameBackend.App.Core.Models;
using MyGame.GameBackend.App.Core.Messages;
using MyGame.GameBackend.App.Core.MessagesDispatchers.Interfaces;
using MyGame.GameBackend.App.Core.Networks.Interfaces;

namespace MyGame.GameBackend.App.Core.MessagesDispatchers
{
    public class RequestHandlerWrapper<TRequest, TResponse> : IRequestHandlerWrapper
    {
        private readonly Func<IClientConnection, TRequest, Task<TResponse>> _handler;

        public RequestHandlerWrapper(Func<IClientConnection, TRequest, Task<TResponse>> handler)
        {
            _handler = handler;
        }

        public async Task HandleAsync(IClientConnection conn, ProtocolEnvelope envelope, Dispatcher dispatcher)
        {
            var payload = MemoryPackSerializer.Deserialize<TRequest>(envelope.Payload);
            var result = await _handler(conn, payload!);

            if (!string.IsNullOrEmpty(envelope.RequestId))
            {
                var response = new ProtocolEnvelope
                {
                    Kind = MessageKind.Response,
                    MessageType = envelope.MessageType,
                    RequestId = envelope.RequestId,
                    Payload = MemoryPackSerializer.Serialize(result)
                };
                await conn.SendEnvelopeAsync(response);
            }
        }
    }
    public class RequestHandlerWrapper<TRequest> : IRequestHandlerWrapper
    {
        private readonly Func<IClientConnection, TRequest, Task> _handler;

        public RequestHandlerWrapper(Func<IClientConnection, TRequest, Task> handler)
        {
            _handler = handler;
        }

        public async Task HandleAsync(IClientConnection conn, ProtocolEnvelope envelope, Dispatcher dispatcher)
        {
            var payload = MemoryPackSerializer.Deserialize<TRequest>(envelope.Payload);
            await _handler(conn, payload!);
        }
    }
}
