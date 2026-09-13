using System.Runtime.InteropServices;
using MauiBlazorHol.Shared.Services;

namespace MauiBlazorHol.Web.Services;

public class PlatformInfo : IPlatformInfo
{
    public PlatformInformation GetInfo()
    {
        return new PlatformInformation
        {
            Model = "Web server",
            Manufacturer = "unknown",
            Version = Environment.OSVersion.ToString(),
            Platform = RuntimeInformation.OSDescription
        };
    }
}
