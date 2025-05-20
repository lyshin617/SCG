using MyGame.GameBackend.App.Core.Auto;
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

        public void RunAsync(CancellationToken token)
        {
            Console.WriteLine("[App] Starting Game Server...");

            // 初始化元件
            _dispatcherRouter = new DispatcherRouter();
            _sessionManager = new InMemorySessionManager();
            _transporter = new Transporter("127.0.0.1", 7777);

            // 綁定 transporter 事件
            _transporter.OnConnect += OnClientConnect;
            _transporter.OnDisconnect += OnClientDisconnect;
            _transporter.OnMessage += async (conn, envelope) =>
            {
                // Dispatcher 處理所有進來封包
                await _dispatcherRouter.DispatchAsync(conn, envelope);
            };

            // 註冊所有 plugin handler
            RegisterPlugins();

            // 啟動 Transporter 在背景
            _ = Task.Run(() => _transporter.StartAsync(token), token);

            Console.WriteLine("[App] Started Game Server At 127.0.0.1:7777");
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
                Thread.Sleep(100); // 降低 CPU 消耗
            }
            Console.WriteLine("[App] Stop");
        }

        private void OnClientConnect(IClientConnection conn)
        {
            Console.WriteLine($"[Transporter] Client connected: {conn.Id}");
            var session = _sessionManager.CreateSession(conn.Id);
            conn.Session = session;
        }

        private void OnClientDisconnect(IClientConnection conn)
        {
            Console.WriteLine($"[Transporter] Client disconnected: {conn.Id}");
            _sessionManager.RemoveSession(conn.Id);
            conn.Session = null;
        }

        private void RegisterPlugins()
        {
            Assembly moduleAssembly = Assembly.LoadFrom("MyGame.GameBackend.Modules.dll");
            AutoDispatcherRegister.RegisterAllRpcHandlers(_dispatcherRouter, moduleAssembly);
        }
    }

}
