using System.Net;

namespace MyGame.GameBackend.App.Core
{
    public interface IClientConnection
    {
        string Id { get; }
        EndPoint? RemoteEndPoint { get; }

        void Dispose();
        Task<ProtocolEnvelope?> ReceiveEnvelopeAsync(CancellationToken token);
        Task SendEnvelopeAsync(ProtocolEnvelope envelope);
    }
}