using MyGame.GameBackend.App.Core.Messages;
using MyGame.GameBackend.App.Core.MessagesDispatchers;
using MyGame.GameBackend.App.Core.Networks;
using MyGame.GameBackend.App.Core.Networks.Interfaces;
using System.Reflection;


namespace MyGame.GameBackend.App.Core
{
    public class App
    {
        private Transporter _transporter;
        private DispatcherRouter _dispatcherRouter;
        private ISessionManager _sessionManager;
        private readonly Dictionary<string, ClientMessageProcessor> _processors = new();

        public void RunAsync(CancellationToken token)
        {
            Console.WriteLine("[App] Starting Game Server...");

            _dispatcherRouter = new DispatcherRouter();
            _sessionManager = new InMemorySessionManager();
            _transporter = new Transporter("127.0.0.1", 7777);

            // 綁定事件
            _transporter.OnConnect += OnClientConnect;
            _transporter.OnDisconnect += OnClientDisconnect;
            _transporter.OnMessage += OnMessage;

            RegisterPlugins();

            _ = Task.Run(() => _transporter.StartAsync(token), token);

            Console.WriteLine("[App] Started Game Server At 127.0.0.1:7777");
        }
        public void Broadcast(ProtocolEnvelope envelope)
        {
            foreach (var processor in _processors.Values)
                processor.Enqueue(envelope); // 或 SendAsync，如果你有送的權限
        }
        public void OnUpdate(CancellationTokenSource cts)
        {
            Console.WriteLine("[App] Running...");
            while (!cts.IsCancellationRequested)
            {
                if (Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Escape)
                {
                    Console.WriteLine("[App] ESC pressed, shutting down...");
                    cts.Cancel();
                    break;
                }
                Thread.Sleep(100);
            }
            Console.WriteLine("[App] Stop");
        }

        private void OnClientConnect(IClientConnection conn)
        {
            Console.WriteLine($"[Transporter] Client connected: {conn.Id}");

            var session = _sessionManager.CreateSession(conn.Id);
            conn.Session = session;

            var processor = new ClientMessageProcessor(conn, _dispatcherRouter);
            _processors[conn.Id] = processor;
        }

        private void OnClientDisconnect(IClientConnection conn)
        {
            Console.WriteLine($"[Transporter] Client disconnected: {conn.Id}");

            _sessionManager.RemoveSession(conn.Id);
            conn.Session = null;

            if (_processors.TryGetValue(conn.Id, out var processor))
            {
                // 可加入 processor.Stop() 處理結束
                _processors.Remove(conn.Id);
            }
        }

        private void OnMessage(IClientConnection conn, ProtocolEnvelope envelope)
        {
            if (_processors.TryGetValue(conn.Id, out var processor))
            {
                processor.Enqueue(envelope);
            }
            else
            {
                Console.WriteLine($"[Warning] No processor found for {conn.Id}");
            }
        }

        private void RegisterPlugins()
        {
            Assembly moduleAssembly = Assembly.LoadFrom("MyGame.GameBackend.Modules.dll");
            ///AutoDispatcherRegister.RegisterAllRpcHandlers(_dispatcherRouter, moduleAssembly);
        }
    }


}
