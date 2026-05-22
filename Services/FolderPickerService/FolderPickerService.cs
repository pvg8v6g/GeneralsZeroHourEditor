using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace GeneralsZeroHourEditor.Services.FolderPickerService;

public class FolderPickerService : IFolderPickerService
{
    private readonly Window _window;

    public FolderPickerService(Window window) => _window = window;

    public async Task<string?> PickGeneralsFolderAsync()
    {
        await ShowInstructionAsync(
            title: "Select Generals folder",
            message: "Please choose the installation folder for Command & Conquer Generals.\n\nExample (Steam): C:\\Program Files (x86)\\Steam\\steamapps\\common\\Command and Conquer Generals");
        return await PickFolderAsync(commitText: "Use Generals Folder");
    }

    public async Task<string?> PickZeroHourFolderAsync()
    {
        await ShowInstructionAsync(
            title: "Select Zero Hour folder",
            message: "Please choose the installation folder for Command & Conquer Generals – Zero Hour.\n\nExample (Steam): C:\\Program Files (x86)\\Steam\\steamapps\\common\\Command & Conquer Generals - Zero Hour");
        return await PickFolderAsync(commitText: "Use Zero Hour Folder");
    }

    private async Task<string?> PickFolderAsync(string? commitText)
    {
        var picker = new FolderPicker();
        picker.FileTypeFilter.Add("*");
        picker.SuggestedStartLocation = PickerLocationId.ComputerFolder;
        InitializeWithWindow.Initialize(picker, WindowNative.GetWindowHandle(_window));

        // WinAppSDK FolderPicker doesn't support setting a title, but the CommitButtonText helps a little in context.
        if (!string.IsNullOrWhiteSpace(commitText))
        {
            try { picker.CommitButtonText = commitText; } catch { /* older runtime, ignore */ }
        }

        var folder = await picker.PickSingleFolderAsync();
        return folder?.Path;
    }

    private async Task ShowInstructionAsync(string title, string message)
    {
        try
        {
            // XamlRoot can be null very early in app startup; guard it to avoid blocking the window from showing.
            var xamlRoot = (_window.Content as FrameworkElement)?.XamlRoot;
            if (xamlRoot == null)
            {
                return; // Skip the instruction dialog if the visual tree isn't ready yet.
            }

            var dialog = new ContentDialog
            {
                Title = title,
                Content = message,
                PrimaryButtonText = "OK",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = xamlRoot
            };

            await dialog.ShowAsync();
        }
        catch
        {
            // If a dialog can't be shown for some reason, just continue.
        }
    }
}
