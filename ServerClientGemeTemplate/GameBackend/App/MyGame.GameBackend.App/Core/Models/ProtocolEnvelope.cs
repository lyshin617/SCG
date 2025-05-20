using MemoryPack;
using MyGame.GameBackend.App.Core.Models;

namespace MyGame.GameBackend.App.Core.Messages
{
    [MemoryPackable]
    public partial class ProtocolEnvelope
    {
        public required MessageType MessageType { get; set; }
        public MessageKind Kind { get; set; }
        public string? RequestId { get; set; }
        public byte[]? Payload { get; set; }
    }
}
