using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Storage;
using MauiBlazorHol.Services;
using MauiBlazorHol.Shared.Services;
using Microsoft.Extensions.Logging;

namespace MauiBlazorHol
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                })
                .UseMauiCommunityToolkit();

            builder.Services.AddSingleton<IFolderPicker>(FolderPicker.Default);

            // Add device-specific services used by the MauiBlazorHol.Shared project
            builder.Services.AddSingleton<IFormFactor, FormFactor>();
            builder.Services.AddSingleton<IFolderPickerService, FolderPickerService>();
            builder.Services.AddTransient<IPlatformInfo, PlatformInfo>();

            builder.Services.AddMauiBlazorWebView();

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
