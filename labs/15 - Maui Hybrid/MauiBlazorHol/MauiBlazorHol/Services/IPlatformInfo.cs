namespace MauiBlazorHol.Services;

public interface IPlatformInfo
{
    PlatformInformation GetInfo();
}

public class PlatformInformation
{
    public string Model { get; set; } = "Unknown";
    public string Manufacturer { get; set; } = "Unknown";
    public string Version { get; set; } = "Unknown";
    public string Platform { get; set; } = "Unknown";

    override public string ToString()
    {
        return $"Model: {Model}, Manufacturer: {Manufacturer}, Version: {Version}, Platform: {Platform}";
    }
}
