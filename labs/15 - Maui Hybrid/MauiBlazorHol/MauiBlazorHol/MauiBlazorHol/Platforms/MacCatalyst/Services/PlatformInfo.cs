using MauiBlazorHol.Shared.Services;
using UIKit;

namespace MauiBlazorHol.Services;

public class PlatformInfo : IPlatformInfo
{
    public PlatformInformation GetInfo()
    {
        var device = UIDevice.CurrentDevice;
        return new PlatformInformation
        {
            Model = device.Model,
            Manufacturer = "Apple",
            Version = device.SystemVersion,
            Platform = device.SystemName
        };
    }
}
