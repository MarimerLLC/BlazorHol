namespace MauiBlazorHol.Shared.Services;

public interface IFolderPickerService
{
    Task<string> PickFolderAsync();
}
