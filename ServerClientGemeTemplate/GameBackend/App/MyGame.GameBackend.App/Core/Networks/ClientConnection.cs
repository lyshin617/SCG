using System.Net.Sockets;
using System.Net;
using MyGame.GameBackend.App.Core.Messages;
using MyGame.GameBackend.App.Core.Models;
using MyGame.GameBackend.App.Core.Networks.Interfaces;

namespace MyGame.GameBackend.App.Core.Networks
{

    public class ClientConnection : IDisposable, IClientConnection
    {
        private static int _idGen = 0;
        public string Id { get; }
        public Session? Session { get; set; }
        private readonly Socket _socket;

        public EndPoint? RemoteEndPoint => _socket.RemoteEndPoint;

        public ClientConnection(Socket clientSocket)
        {
            _socket = clientSocket;
            Id = Interlocked.Increment(ref _idGen).ToString(); // 保證唯一
        }

        // 封包最大長度
        private const int MaxPacketSize = 1024 * 1024;

        // 傳送封包
        public async Task SendEnvelopeAsync(ProtocolEnvelope envelope)
        {
            // MemoryPack序列化
            var data = MemoryPack.MemoryPackSerializer.Serialize(envelope);
            int totalLen = 4 + data.Length;
            var pool = System.Buffers.ArrayPool<byte>.Shared;
            byte[] sendBuffer = pool.Rent(totalLen);
            try
            {
                // 長度 prefix
                BitConverter.GetBytes(data.Length).CopyTo(sendBuffer, 0);
                // 序列化資料
                Array.Copy(data, 0, sendBuffer, 4, data.Length);
                await _socket.SendAsync(new ArraySegment<byte>(sendBuffer, 0, totalLen), SocketFlags.None);
            }
            finally
            {
                pool.Return(sendBuffer);
            }
        }
        public void SendEnvelope(ProtocolEnvelope envelope)
        {
            // MemoryPack序列化
            var data = MemoryPack.MemoryPackSerializer.Serialize(envelope);
            int totalLen = 4 + data.Length;
            var pool = System.Buffers.ArrayPool<byte>.Shared;
            byte[] sendBuffer = pool.Rent(totalLen);
            try
            {
                // 長度 prefix
                BitConverter.GetBytes(data.Length).CopyTo(sendBuffer, 0);
                // 序列化資料
                Array.Copy(data, 0, sendBuffer, 4, data.Length);

                int sent = 0;
                while (sent < totalLen)
                {
                    int n = _socket.Send(sendBuffer, sent, totalLen - sent, SocketFlags.None);
                    if (n <= 0)
                        throw new SocketException((int)SocketError.ConnectionReset);
                    sent += n;
                }
            }
            finally
            {
                pool.Return(sendBuffer);
            }
        }


        // 接收封包（回傳一個封包，沒有的話回 null，斷線時亦回 null）
        public async Task<ProtocolEnvelope?> ReceiveEnvelopeAsync(CancellationToken token)
        {
            var pool = System.Buffers.ArrayPool<byte>.Shared;

            // 讀長度
            byte[] lenBuffer = pool.Rent(4);
            int read = 0;
            try
            {
                while (read < 4)
                {
                    int r = await _socket.ReceiveAsync(new ArraySegment<byte>(lenBuffer, read, 4 - read), SocketFlags.None, token);
                    if (r == 0) return null; // 斷線
                    read += r;
                }
                int bodyLen = BitConverter.ToInt32(lenBuffer, 0);
                if (bodyLen <= 0 || bodyLen > MaxPacketSize) return null;

                // 讀資料
                byte[] dataBuffer = pool.Rent(bodyLen);
                try
                {
                    read = 0;
                    while (read < bodyLen)
                    {
                        int r = await _socket.ReceiveAsync(new ArraySegment<byte>(dataBuffer, read, bodyLen - read), SocketFlags.None, token);
                        if (r == 0) return null;
                        read += r;
                    }
                    var envelope = MemoryPack.MemoryPackSerializer.Deserialize<ProtocolEnvelope>(dataBuffer.AsSpan(0, bodyLen));
                    return envelope;
                }
                finally
                {
                    pool.Return(dataBuffer);
                }
            }
            finally
            {
                pool.Return(lenBuffer);
            }
        }


        public void Dispose()
        {
            try { _socket.Shutdown(SocketShutdown.Both); } catch { }
            try { _socket.Close(); } catch { }
        }
    }

}
