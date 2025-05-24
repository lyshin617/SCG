using MemoryPack;
using MyGame.GameBackend.App.Core.Messages;
using MyGame.GameBackend.App.Core.MessagesDispatchers;
using MyGame.GameBackend.App.Core.Models;
using System;
using System.Reflection;
namespace MyGame.GameBackend.App.Core.CustomAttributes
{
    public static class RpcHandlerRegistrar
    {
        public static void RegisterAllHandlers(DispatcherRouter router, Assembly assembly)
        {
            foreach (var type in assembly.GetTypes())
            {
                var moduleName = GetModuleName(type);
                if (moduleName == null) continue;

                var dispatcher = router.GetOrCreateModule(moduleName);
                var instance = Activator.CreateInstance(type);
                if (instance == null) continue;

                foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static))
                {
                    var attr = method.GetCustomAttribute<RpcAttribute>();
                    if (attr == null) continue;

                    switch (attr.Kind)
                    {
                        case MessageKind.Request:
                            RegisterRequestHandler(dispatcher, attr.Action, instance, method);
                            break;
                        case MessageKind.Response:
                            RegisterResponseHandler(dispatcher, attr.Action, instance, method);
                            break;
                        case MessageKind.Event:
                            RegisterEventHandler(dispatcher, attr.Action, instance, method);
                            break;
                    }
                }
            }
        }
        private static string? GetModuleName(Type type)
        {
            var attr = type.GetCustomAttribute<RpcModuleAttribute>();
            return attr?.Module;
        }


        private static void RegisterRequestHandler(Dispatcher dispatcher, string action, object target, MethodInfo method)
        {
            dispatcher.RegisterRequestHandler(action, async ctx =>
            {
                var payloadType = method.GetParameters().First().ParameterType;
                var payload = DeserializePayload(ctx.Envelope.Payload, payloadType);

                var resultTask = (Task)method.Invoke(target, new[] { payload })!;
                await resultTask.ConfigureAwait(false);

                var resultProp = resultTask.GetType().GetProperty("Result")!;
                var result = resultProp.GetValue(resultTask);

                // 使用非泛型封裝 response
                var returnType = resultProp.PropertyType;
                return EnvelopeUtils.CreateResponse(ctx.Envelope, result!, returnType);
            });
        }

        private static void RegisterResponseHandler(Dispatcher dispatcher, string action, object target, MethodInfo method)
        {
            dispatcher.RegisterResponseHandler(action, ctx =>
            {
                var payloadType = method.GetParameters().First().ParameterType;
                var payload = DeserializePayload(ctx.Envelope.Payload, payloadType);
                method.Invoke(target, new[] { payload });
            });
        }

        private static void RegisterEventHandler(Dispatcher dispatcher, string action, object target, MethodInfo method)
        {
            dispatcher.RegisterEventHandler(action, ctx =>
            {
                var payloadType = method.GetParameters().First().ParameterType;
                var payload = DeserializePayload(ctx.Envelope.Payload, payloadType);
                method.Invoke(target, new[] { payload });
            });
        }

        private static object DeserializePayload(byte[]? payloadBytes, Type type)
        {
            var deserializeMethod = typeof(MemoryPackSerializer)
                .GetMethods()
                .First(m => m.Name == "Deserialize" && m.IsGenericMethod && m.GetParameters().Length == 1)
                .MakeGenericMethod(type);

            return deserializeMethod.Invoke(null, new object?[] { payloadBytes })!;
        }
    }

}
