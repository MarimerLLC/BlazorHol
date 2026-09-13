using MauiBlazorHol.Shared.Services;

namespace MauiBlazorHol.Web.Services;

public class FolderPickerService : IFolderPickerService
{
    public Task<string> PickFolderAsync()
    {
        return Task.FromResult("Picking a folder is not supported in the web app");
    }
}
