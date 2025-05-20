using MyGame.GameBackend.App.Core;

var cts = new CancellationTokenSource();

AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
{
    Console.WriteLine($"[FATAL] UnhandledException: {e.ExceptionObject}");
};

TaskScheduler.UnobservedTaskException += (sender, e) =>
{
    Console.WriteLine($"[FATAL] UnobservedTaskException: {e.Exception}");
    e.SetObserved();
};

try
{
    var app = new App();
    app.RunAsync(cts.Token);
    app.OnUpdate(cts);
}
catch (Exception ex)
{
    Console.WriteLine($"[ERROR] Fatal exception: {ex}");
}
