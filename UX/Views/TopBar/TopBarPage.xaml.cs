using GeneralsZeroHourEditor.AppMain;
using GeneralsZeroHourEditor.UX.ViewModels.TopBar;
using Microsoft.Extensions.DependencyInjection;

namespace GeneralsZeroHourEditor.UX.Views.TopBar;

public sealed partial class TopBarPage
{
    public TopBarViewModel ViewModel { get; }

    public TopBarPage()
    {
        InitializeComponent();
        ViewModel = App.Services!.GetRequiredService<TopBarViewModel>();
        DataContext = ViewModel;
    }
}
