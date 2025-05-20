using MyGame.GameBackend.App.Core.Messages;
using MyGame.GameBackend.App.Core.Networks.Interfaces;

namespace MyGame.GameBackend.App.Core.MessagesDispatchers.Interfaces
{
    internal interface IDispatcher
    {
        Task BroadcastRequestAsync<T>(IEnumerable<IClientConnection> targets, MessageType messageType, T payload);
        Task DispatchAsync(IClientConnection conn, ProtocolEnvelope envelope);
        void RegisterRequestHandler<TRequest, TResponse>(string messageType, Func<IClientConnection, TRequest, Task<TResponse>> handler);
        void RegisterRequestHandler<TRequest>(string action, Func<IClientConnection, TRequest, Task> handler);
        Task<TResponse> SendRequestAsync<TRequest, TResponse>(IClientConnection conn, MessageType messageType, TRequest payload);
        Task SendRequestAsync<TRequest>(IClientConnection conn, MessageType messageType, TRequest payload);
    }
}