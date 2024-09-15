namespace MauiBlazorHol.Services;

public class PlatformInfo : IPlatformInfo
{
    public PlatformInformation GetInfo()
    {
        return new PlatformInformation
        {
            Model = Android.OS.Build.Model ?? "unknown",
            Manufacturer = Android.OS.Build.Manufacturer ?? "unknown",
            Version = Android.OS.Build.VERSION.Release ?? "unknown",
            Platform = "Android"
        };
    }
}
