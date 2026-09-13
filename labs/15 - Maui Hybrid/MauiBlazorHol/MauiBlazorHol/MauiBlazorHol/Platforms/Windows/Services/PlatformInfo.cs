using System.Runtime.InteropServices;
using MauiBlazorHol.Shared.Services;

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
