using System.Net.Http.Json;

namespace BlazorHolState.Client;

/// <summary>
/// Dictionary containing per-user session objects, keyed
/// by sessionId.
/// </summary>
public class SessionManager(HttpClient client) : ISessionManager
{
    private Session? _session;

    public async Task<Session> GetSessionAsync()
    {
        _session = await client.GetFromJsonAsync<Session>("state");
        if (_session == null)
            throw new InvalidOperationException("Session not found");
        return _session;
    }

    public async Task UpdateSessionAsync(Session session)
    {
        await client.PutAsJsonAsync<Session>("state", session);
        _session = session;
    }
}
