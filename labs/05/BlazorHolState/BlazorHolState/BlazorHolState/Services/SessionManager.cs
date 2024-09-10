using System.Threading;

namespace BlazorHolState.Server;

/// <summary>
/// Dictionary containing per-user session objects, keyed
/// by sessionId.
/// </summary>
public class SessionManager(SessionIdManager sessionIdManager) : ISessionManager
{ 
    private Dictionary<string, Session> _sessions = new Dictionary<string, Session>();

    public async Task<Session> GetSessionAsync()
    {
        var key = await sessionIdManager.GetSessionIdAsync();
        if (!_sessions.ContainsKey(key))
            _sessions.Add(key, new Session());
        var session = _sessions[key];
        session.SessionId = key;
        var endTime = DateTime.Now + TimeSpan.FromSeconds(10);
        return session;
    }

    public async Task UpdateSessionAsync(Session session)
    {
        if (session != null)
        {
            var key = await sessionIdManager.GetSessionIdAsync();
            session.SessionId = key;
            Replace(session, _sessions[key]);
        }
    }

    /// <summary>
    /// Replace the contents of oldSession with the items
    /// in newSession.
    /// </summary>
    /// <param name="newSession"></param>
    /// <param name="oldSession"></param>
    private void Replace(Session newSession, Session oldSession)
    {
        oldSession.Clear();
        foreach (var key in newSession.Keys)
            oldSession.Add(key, newSession[key]);
    }
}
