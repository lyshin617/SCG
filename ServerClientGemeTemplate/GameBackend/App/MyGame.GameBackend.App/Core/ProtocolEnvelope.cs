using MemoryPack;

namespace MyGame.GameBackend.App.Core
{
    [MemoryPackable]
    public partial class ProtocolEnvelope
    {
        //public MessageKind Kind { get; set; }
        public string? RequestId { get; set; }
        public byte[] Payload { get; set; }
    }
}
