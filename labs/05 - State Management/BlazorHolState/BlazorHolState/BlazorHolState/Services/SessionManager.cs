namespace BlazorHolState.Server;

/// <summary>
/// Dictionary containing per-user session objects, keyed
/// by sessionId.
/// </summary>
public class SessionManager(SessionIdManager sessionIdManager) : ISessionManager
{ 
    private readonly Dictionary<string, Session> _sessions = [];

    public async Task<Session> GetSessionAsync()
    {
        var key = await sessionIdManager.GetSessionIdAsync();
        if (!_sessions.ContainsKey(key))
            _sessions.Add(key, []);
        var session = _sessions[key];
        session.SessionId = key;
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
    private static void Replace(Session newSession, Session oldSession)
    {
        if (ReferenceEquals(newSession, oldSession))
            return;
        oldSession.Clear();
        foreach (var key in newSession.Keys)
            oldSession.Add(key, newSession[key]);
    }
}
