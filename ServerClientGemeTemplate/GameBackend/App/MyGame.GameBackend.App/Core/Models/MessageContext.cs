using MemoryPack;
using MyGame.GameBackend.App.Core.Models;

namespace MyGame.GameBackend.App.Core.Messages
{
    public class MessageContext
    {
        public ProtocolEnvelope Envelope { get; }
        public Session? Session { get; }

        public MessageContext(ProtocolEnvelope envelope, Session? session)
        {
            Envelope = envelope;
            Session = session;
        }

        public TPayload GetPayload<TPayload>()
         => MemoryPackSerializer.Deserialize<TPayload>(Envelope.Payload) ?? throw new InvalidOperationException("Invalid payload");
    }

}
