using GeneralsZeroHourEditor.Tasks;
using Microsoft.UI.Xaml.Controls;

namespace GeneralsZeroHourEditor.Services.NavigationService;

public interface INavigationService
{
    Frame? TopFrame { get; set; }

    Frame? ActiveFrame { get; set; }

    void SetTopBar<T>() where T : Page;

    void NavigateTo<T>() where T : Page;

    Task ShowProgressPopup<T>(string? label) where T : EngineTask;
}
