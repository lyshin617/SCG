using MemoryPack;

namespace MyGame.GameBackend.App.Core.Messages
{
    [MemoryPackable]
    public partial class MessageType
    {
        public string Module;
        public string Action;
    }
}
