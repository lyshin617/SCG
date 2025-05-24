using MyGame.GameBackend.App.Core.Messages;


namespace MyGame.GameBackend.App.Core.MessagesDispatchers
{
    public class DispatcherRouter
    {
        private readonly Dictionary<string, Dispatcher> _moduleDispatchers = new();

        internal Dispatcher GetOrCreateModule(string moduleName)
        {
            if (!_moduleDispatchers.TryGetValue(moduleName, out var dispatcher))
            {
                dispatcher = new Dispatcher();
                _moduleDispatchers[moduleName] = dispatcher;
            }
            return dispatcher;
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
