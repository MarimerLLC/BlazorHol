using System.Runtime.InteropServices;

namespace MauiBlazorHol.Services;

public class PlatformInfo : IPlatformInfo
{
    public PlatformInformation GetInfo()
    {
        return new PlatformInformation
        {
            Model = "Unknown PC",
            Manufacturer = "unknown",
            Version = Environment.OSVersion.ToString(),
            Platform = RuntimeInformation.OSDescription
        };
    }
}
