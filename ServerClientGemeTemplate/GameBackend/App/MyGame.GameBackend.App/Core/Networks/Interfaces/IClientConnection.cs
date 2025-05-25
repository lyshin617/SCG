using MyGame.GameBackend.App.Core.Models;
using MyGame.GameBackend.App.Core.Messages;
using System.Net;

namespace MyGame.GameBackend.App.Core.Networks.Interfaces
{
    public interface IClientConnection
    {
        string Id { get; }
        Session Session { get; set; }
        EndPoint? RemoteEndPoint { get; }

        void Dispose();
        Task<ProtocolEnvelope?> ReceiveEnvelopeAsync(CancellationToken token);
        Task SendEnvelopeAsync(ProtocolEnvelope envelope);
        void SendEnvelope(ProtocolEnvelope envelope);
    }
}