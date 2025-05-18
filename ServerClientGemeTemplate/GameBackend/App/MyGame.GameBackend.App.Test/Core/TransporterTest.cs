using Moq;
using System.Net;
using System.Net.Sockets;

namespace MyGame.GameBackend.App.Core
{
    [TestClass]
    public sealed class TransporterTest
    {
        private Transporter? _transporter;

        [TestCleanup]
        public void Cleanup()
        {
            _transporter?.Stop();
        }

        [TestMethod]
        public async Task Transporter_BasicCommunication_Works()
        {
            int port = 33333; // 可隨機分配
            _transporter = new Transporter("127.0.0.1", port);

            ProtocolEnvelope? received = null;
            _transporter.OnMessage += (conn, envelope) =>
            {
                received = envelope;
            };

            // 啟動 server
            var cts = new CancellationTokenSource();
            var serverTask = _transporter.StartAsync(cts.Token);

            // 準備 client，連線 server
            using var client = new TcpClient();
            await client.ConnectAsync("127.0.0.1", port);
            using var stream = client.GetStream();

            // 模擬 client 發送協定封包
            var envelope = new ProtocolEnvelope
            {
                RequestId = "1",
                Payload = MemoryPack.MemoryPackSerializer.Serialize("hello world!")
            };
            // 包裝+發送
            var data = MemoryPack.MemoryPackSerializer.Serialize(envelope);
            var lengthPrefix = BitConverter.GetBytes(data.Length);
            await stream.WriteAsync(lengthPrefix, 0, 4);
            await stream.WriteAsync(data, 0, data.Length);

            // 等待 server 處理
            await Task.Delay(300);

            Assert.IsNotNull(received);
            Assert.AreEqual("1", received.RequestId);

            // 關閉 server
            cts.Cancel();
            await serverTask;
            // 不用呼叫 Stop，會自動於 Cleanup 做
        }

        [TestMethod]
        public void Stop_Should_CloseServerSocket_And_ClearConnections()
        {
            int port = 33334;
            _transporter = new Transporter("127.0.0.1", port);

            var mockConn1 = new Mock<IClientConnection>();
            var mockConn2 = new Mock<IClientConnection>();
            mockConn1.SetupGet(c => c.Id).Returns("c1");
            mockConn2.SetupGet(c => c.Id).Returns("c2");
            mockConn1.Setup(c => c.Dispose());
            mockConn2.Setup(c => c.Dispose());

            _transporter.AddTestConnection(mockConn1.Object);
            _transporter.AddTestConnection(mockConn2.Object);

            _transporter.Stop();

            Assert.AreEqual(0, _transporter.Connections.Count);
            mockConn1.Verify(c => c.Dispose(), Times.Once);
            mockConn2.Verify(c => c.Dispose(), Times.Once);

            Assert.ThrowsException<ObjectDisposedException>(() =>
            {
                _transporter.BindSocket(new IPEndPoint(IPAddress.Loopback, port));
            });
        }

        [TestMethod]
        public async Task SendAsync_Should_Call_SendEnvelopeAsync_OnClientConnection()
        {
            _transporter = new Transporter("127.0.0.1", 33335);
            var mockConn = new Mock<IClientConnection>();
            mockConn.SetupGet(c => c.Id).Returns("abc");
            var envelope = new ProtocolEnvelope { Payload = new byte[0] };
            mockConn.Setup(c => c.SendEnvelopeAsync(envelope)).Returns(Task.CompletedTask).Verifiable();

            _transporter.AddTestConnection(mockConn.Object);

            await _transporter.SendAsync(mockConn.Object, envelope);

            mockConn.Verify(c => c.SendEnvelopeAsync(envelope), Times.Once());
        }

        [TestMethod]
        public async Task SendAllAsync_Should_SendToAllConnections()
        {
            _transporter = new Transporter("127.0.0.1", 33336);
            var mockConn1 = new Mock<IClientConnection>();
            var mockConn2 = new Mock<IClientConnection>();
            mockConn1.SetupGet(c => c.Id).Returns("a1");
            mockConn2.SetupGet(c => c.Id).Returns("a2");

            var envelope = new ProtocolEnvelope { Payload = new byte[0] };

            mockConn1.Setup(c => c.SendEnvelopeAsync(envelope)).Returns(Task.CompletedTask).Verifiable();
            mockConn2.Setup(c => c.SendEnvelopeAsync(envelope)).Returns(Task.CompletedTask).Verifiable();

            _transporter.AddTestConnection(mockConn1.Object);
            _transporter.AddTestConnection(mockConn2.Object);

            await _transporter.SendAllAsync(envelope);

            mockConn1.Verify(c => c.SendEnvelopeAsync(envelope), Times.Once());
            mockConn2.Verify(c => c.SendEnvelopeAsync(envelope), Times.Once());
        }
    }
}
