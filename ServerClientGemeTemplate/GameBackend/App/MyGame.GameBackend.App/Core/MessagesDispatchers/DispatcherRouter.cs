using MyGame.GameBackend.App.Core.Messages;
using MyGame.GameBackend.App.Core.MessagesDispatchers.Interfaces;
using MyGame.GameBackend.App.Core.Networks.Interfaces;

namespace MyGame.GameBackend.App.Core.MessagesDispatchers
{
    public class DispatcherRouter
    {
        private readonly Dictionary<string, IDispatcher> _moduleDispatchers = new();

        internal void RegisterDispatcher(string modulePrefix, IDispatcher dispatcher)
        {
            _moduleDispatchers[modulePrefix] = dispatcher;
        }

        public async Task DispatchAsync(IClientConnection conn, ProtocolEnvelope envelope)
        {
            if (_moduleDispatchers.TryGetValue(envelope.MessageType.Module, out IDispatcher? dispatcher))
            {
                await dispatcher.DispatchAsync(conn, envelope);
            }
            else
            {
                Console.WriteLine($"[WARN] No dispatcher found for message type: {envelope.MessageType}");
            }
        }
    }
}
