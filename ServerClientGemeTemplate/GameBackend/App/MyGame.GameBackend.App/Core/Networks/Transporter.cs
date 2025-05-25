using MyGame.GameBackend.App.Core.Messages;
using MyGame.GameBackend.App.Core.Networks.Interfaces;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;

namespace MyGame.GameBackend.App.Core.Networks
{
    public class Transporter
    {
        public event Action<IClientConnection>? OnConnect;
        public event Action<IClientConnection>? OnDisconnect;
        public event Action<IClientConnection, ProtocolEnvelope>? OnMessage;

        private const int backlog = 100;
        private readonly ConcurrentDictionary<string, IClientConnection> _connections = new();
        private Socket _serverSocket;

        public IReadOnlyCollection<IClientConnection> Connections => _connections.Values.ToList().AsReadOnly();

        public Transporter(string ip, int port)
        {
            var ipAddress = IPAddress.Parse(ip);
            IPEndPoint serverEndPoint = new(ipAddress, port);
            _serverSocket = new Socket(serverEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _serverSocket.Bind(serverEndPoint);
            _serverSocket.Listen(backlog);
        }

        public async Task StartAsync(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    var acceptTask = _serverSocket.AcceptAsync();
                    var completed = await Task.WhenAny(acceptTask, Task.Delay(Timeout.Infinite, token));
                    if (completed == acceptTask)
                    {
                        var client = acceptTask.Result;
                        var connection = new ClientConnection(client);

                        _connections.TryAdd(connection.Id, connection);
                        OnConnect?.Invoke(connection);
                        //todo: Handle task
                        Task run = Task.Run(() => HandleClientAsync(connection, token), token);
                        
                    }
                    else
                    {
                        // Cancellation triggered
                        break;
                    }
                }
            }
            catch (ObjectDisposedException)
            {
                Console.WriteLine("[INFO] Server socket closed, accept loop stopped.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Transporter accept loop exception: {ex.Message}");
            }
        }



        private async Task HandleClientAsync(IClientConnection connection, CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    var envelope = await connection.ReceiveEnvelopeAsync(token);
                    if (envelope != null)
                    {
                        OnMessage?.Invoke(connection, envelope);
                    }
                    else
                    {
                        break; // Client disconnected
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] HandleClientAsync Exception for client {connection.Id} (Remote: {connection.RemoteEndPoint}) - {ex.Message}");
            }
            finally
            {
                _connections.TryRemove(connection.Id, out _);
                OnDisconnect?.Invoke(connection);
                connection.Dispose();
            }
        }

        public void Stop()
        {
            _serverSocket.Close();
            foreach (var conn in _connections.Values)
                conn.Dispose();
            _connections.Clear();
        }

        public Task SendAsync(IClientConnection connection, ProtocolEnvelope envelope)
        {
            return connection.SendEnvelopeAsync(envelope);
        }

        public async Task SendAllAsync(ProtocolEnvelope envelope)
        {
            var tasks = _connections.Values.Select(conn => SendAsync(conn, envelope));
            await Task.WhenAll(tasks);
        }
        public void Send(IClientConnection connection, ProtocolEnvelope envelope)
        {
            connection.SendEnvelope(envelope);
        }


#if DEBUG
        public void AddTestConnection(IClientConnection conn)
        {
            _connections.TryAdd(conn.Id, conn);
        }
        public void BindSocket(IPEndPoint iPEndPoint)
        {
            _serverSocket.Bind(iPEndPoint);
        }
#endif

    }

}
