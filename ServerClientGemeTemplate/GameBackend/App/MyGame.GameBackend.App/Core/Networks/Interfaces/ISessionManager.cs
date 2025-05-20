using MyGame.GameBackend.App.Core.Models;

namespace MyGame.GameBackend.App.Core.Networks.Interfaces
{
    public interface ISessionManager
    {
        Session CreateSession(string connectionId, string? playerId = null);
        Session? GetSession(string connectionId);
        void RemoveSession(string connectionId);
        IEnumerable<Session> GetAllSessions();
    }

}
