using MyGame.GameBackend.App.Core.CustomAttributes;
using MyGame.GameBackend.App.Core.MessagesDispatchers;
using MyGame.GameBackend.App.Core.MessagesDispatchers.Interfaces;
using MyGame.GameBackend.App.Core.Networks.Interfaces;
using System.Reflection;

namespace MyGame.GameBackend.App.Core.Auto
{

    public static class AutoDispatcherRegister
    {
        public static void RegisterAllRpcHandlers(DispatcherRouter router, Assembly assembly)
        {
            var dispatchers = new Dictionary<string, Dispatcher>();

            foreach (var type in assembly.GetTypes())
            {
                var moduleAttr = type.GetCustomAttribute<RpcModuleAttribute>();
                if (moduleAttr == null) continue; 

                var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Static);
                foreach (var method in methods)
                {
                    var rpcAttr = method.GetCustomAttribute<RpcAttribute>();
                    if (rpcAttr == null) continue;

                    string module = type.Namespace?.Split('.').Last() ?? "Unknown";

                    if (!dispatchers.TryGetValue(module, out var dispatcher))
                    {
                        dispatcher = new Dispatcher();
                        dispatchers[module] = dispatcher;
                        router.RegisterDispatcher(module, dispatcher);
                    }

                    RegisterMethodToDispatcher(method, dispatcher, $"{module}.{rpcAttr.Action}");
                }
            }
        }
        private static void RegisterMethodToDispatcher(MethodInfo method, Dispatcher dispatcher, string fullAction)
        {
            var parameters = method.GetParameters();
            if (parameters.Length != 2 || !typeof(IClientConnection).IsAssignableFrom(parameters[0].ParameterType))
                throw new InvalidOperationException($"Invalid RPC handler signature: {method.Name}");

            var requestType = parameters[1].ParameterType;
            var returnType = method.ReturnType;

            if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>))
            {
                var responseType = returnType.GetGenericArguments()[0];
                var wrapper = Activator.CreateInstance(
                    typeof(RequestHandlerWrapper<,>).MakeGenericType(requestType, responseType),
                    Delegate.CreateDelegate(typeof(Func<,,>).MakeGenericType(typeof(IClientConnection), requestType, returnType), method)
                )!;
                dispatcher.RegisterRequestHandler(fullAction, (IRequestHandlerWrapper)wrapper);
            }
            else if (returnType == typeof(Task))
            {
                var wrapper = Activator.CreateInstance(
                    typeof(RequestHandlerWrapper<>).MakeGenericType(requestType),
                    Delegate.CreateDelegate(typeof(Func<,>).MakeGenericType(typeof(IClientConnection), requestType, returnType), method)
                )!;
                dispatcher.RegisterRequestHandler(fullAction, (IRequestHandlerWrapper)wrapper);
            }
            else
            {
                throw new InvalidOperationException($"Unsupported return type: {method.Name}");
            }
        }

    }


}
