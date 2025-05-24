using MyGame.GameBackend.App.Core.Messages;

namespace MyGame.GameBackend.App.Core.MessagesDispatchers
{
    public class DispatchResult
    {
        public bool ShouldReply { get; private set; }
        public bool ShouldDrop { get; private set; }
        public ProtocolEnvelope? Response { get; private set; }

        public static DispatchResult Reply(ProtocolEnvelope response) =>
            new() { ShouldReply = true, Response = response };

        public static DispatchResult Drop() => new() { ShouldDrop = true };
        public static DispatchResult NoReply() => new();
    }
}