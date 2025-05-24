using MemoryPack;

namespace MyGame.GameBackend.App.Core.Messages
{
    [MemoryPackable]
    public partial class ErrorResponse
    {
        public required string Code { get; set; }
        public required string Message { get; set; }
        public string? Detail { get; set; }
    }

}
