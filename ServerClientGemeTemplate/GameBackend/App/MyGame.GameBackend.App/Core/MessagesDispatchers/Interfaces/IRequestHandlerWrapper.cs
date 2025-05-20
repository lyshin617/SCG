using MyGame.GameBackend.App.Core.Messages;
using MyGame.GameBackend.App.Core.Networks.Interfaces;

namespace MyGame.GameBackend.App.Core.MessagesDispatchers.Interfaces
{
    internal interface IRequestHandlerWrapper
    {
        Task HandleAsync(IClientConnection conn, ProtocolEnvelope envelope, Dispatcher dispatcher);
    }

}
