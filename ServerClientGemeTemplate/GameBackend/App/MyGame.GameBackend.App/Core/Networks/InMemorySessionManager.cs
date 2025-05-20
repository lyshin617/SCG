using MyGame.GameBackend.App.Core.Models;
using MyGame.GameBackend.App.Core.Networks.Interfaces;
using System.Collections.Concurrent;

namespace MyGame.GameBackend.App.Core.Networks
{
    public class InMemorySessionManager : ISessionManager
    {
        private readonly ConcurrentDictionary<string, Session> _sessions = new();

        public Session CreateSession(string connectionId, string? playerId = null)
        {
            var session = new Session
            {
                SessionId = Guid.NewGuid().ToString("N"),
                ConnectionId = connectionId,
                PlayerId = playerId
            };
            _sessions[connectionId] = session;
            return session;
        }

        public Session? GetSession(string connectionId)
            => _sessions.TryGetValue(connectionId, out var session) ? session : null;

        public void RemoveSession(string connectionId)
            => _sessions.TryRemove(connectionId, out _);

        public IEnumerable<Session> GetAllSessions()
            => _sessions.Values;
    }


}
