using MyGame.GameBackend.App.Core.Messages;
using MyGame.GameBackend.App.Core.MessagesDispatchers.Interfaces;

namespace MyGame.GameBackend.App.Core.MessagesDispatchers
{
    public class DispatcherRouter
    {
        private readonly Dictionary<string, IDispatcher> _moduleDispatchers = new();

        internal void RegisterDispatcher(string modulePrefix, IDispatcher dispatcher)
        {
            _moduleDispatchers[modulePrefix] = dispatcher;
        }

        public async Task<DispatchResult> RouteAsync(MessageContext ctx)
        {
            var moduleKey = ctx.Envelope.MessageType.Module;
            if (_moduleDispatchers.TryGetValue(moduleKey, out var dispatcher))
            {
                return await dispatcher.DispatchAsync(ctx);
            }

            return DispatchResult.Drop(); // 模組找不到就丟掉或回錯
        }
    }
}
