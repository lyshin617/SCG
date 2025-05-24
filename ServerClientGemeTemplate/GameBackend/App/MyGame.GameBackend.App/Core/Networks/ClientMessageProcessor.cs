using MyGame.GameBackend.App.Core.Messages;
using MyGame.GameBackend.App.Core.Networks.Interfaces;
using System.Threading.Channels;
using MyGame.GameBackend.App.Core.MessagesDispatchers;
using MyGame.GameBackend.App.Core.Models;

namespace MyGame.GameBackend.App.Core.Networks
{
    // 此類別負責管理每個 Client 封包的佇列與處理流程
    internal class ClientMessageProcessor
    {
        private readonly IClientConnection _conn;
        private readonly DispatcherRouter _router;
        private readonly Channel<ProtocolEnvelope> _queue;
        private Task _processingTask;
        private readonly CancellationTokenSource _cts = new();

        internal ClientMessageProcessor(IClientConnection conn, DispatcherRouter router)
        {
            _conn = conn;
            _router = router;
            _queue = Channel.CreateBounded<ProtocolEnvelope>(new BoundedChannelOptions(100)
            {
                FullMode = BoundedChannelFullMode.DropOldest // 或 DropWrite 根據封包性質調整
            });

            _processingTask = Task.Run(async () =>
            {
                try
                {
                    await ProcessLoop(_cts.Token);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[FatalError in ProcessLoop] {ex}");
                }
            }); // 啟動處理任務
        }
        public void Stop()
        {
            _cts.Cancel();
            _processingTask.Wait();
        }
        public void Enqueue(ProtocolEnvelope envelope)
        {
            _queue.Writer.TryWrite(envelope);
        }

        private async Task ProcessLoop(CancellationToken cancellationToken)
        {
            await foreach (var envelope in _queue.Reader.ReadAllAsync(cancellationToken))
            {
                try
                {
                    var session = _conn.Session;

                    // 如果此封包需要 session，但目前 session 為 null，回應未登入
                    if (RequiresSession(envelope) && session == null)
                    {
                        Console.WriteLine($"[Reject] Session is null for: {envelope.MessageType.Action}");
                        await _conn.SendEnvelopeAsync(EnvelopeUtils.CreateErrorResponse(envelope, "未登入"));
                        continue;
                    }

                    // 建立 context（允許 session 為 null，視 handler 決定是否接受）
                    var ctx = new MessageContext(envelope, session);
                    var result = await _router.RouteAsync(ctx);

                    if (result.ShouldReply)
                        await _conn.SendEnvelopeAsync(result.Response);
                    else if (result.ShouldDrop)
                        continue;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[DispatchError] {ex.Message}");
                    await _conn.SendEnvelopeAsync(EnvelopeUtils.CreateErrorResponse(envelope, ex));
                }

            }
        }
        private bool RequiresSession(ProtocolEnvelope env)
        {
            // 視你的協議設計，列出例外項目
            var publicActions = new[] { "Login", "Ping", "Handshake" };
            return env.Kind == MessageKind.Request &&
                   !publicActions.Contains(env.MessageType.Action);
        }
        

    }

}
