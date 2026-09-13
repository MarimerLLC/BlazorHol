using CommunityToolkit.Maui.Storage;
using MauiBlazorHol.Shared.Services;

namespace MauiBlazorHol.Services;

public class FolderPickerService(IFolderPicker folderPicker) : IFolderPickerService
{
    public async Task<string> PickFolderAsync()
    {
        var result = await folderPicker.PickAsync();
        if (result.IsSuccessful)
        {
            return $"Picked folder: {result.Folder.Path}";
        }
        return $"No folder picked: {result.Exception?.Message}";
    }
}
