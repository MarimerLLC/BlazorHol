namespace BlazorHolState;

public interface ISessionManager
{
    Task<Session> GetSessionAsync();
    Task UpdateSessionAsync(Session session);
}
