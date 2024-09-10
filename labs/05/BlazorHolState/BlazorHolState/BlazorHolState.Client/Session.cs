namespace BlazorHolState;

/// <summary>
/// Per-user session data. The object must be 
/// serializable via JSON.
/// </summary>
public class Session : Dictionary<string, string>
{
    /// <summary>
    /// Gets or sets the Session Id value.
    /// </summary>
    public string SessionId { get => this["__sessionId"]; set => this["__sessionId"] = value; }
}
