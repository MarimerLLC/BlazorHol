namespace BlazorStyles.Data;

public class UserInfo
{
    public bool IsAuthenticated => Claims.Count != 0;
    public string Name { get; set; } = string.Empty;
    public List<ClaimInfo> Claims { get; set; } = [];
}

public class ClaimInfo
{
    public ClaimInfo() { }

    public ClaimInfo(string type, string value)
    {
        Type = type;
        Value = value;
    }

    public string Type { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}
