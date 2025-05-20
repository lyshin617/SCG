namespace MyGame.GameBackend.App.Core.Models
{
    public class Session
    {
        public required string SessionId { get; set; }
        public required string ConnectionId { get; set; }
        public string? PlayerId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    }
}
