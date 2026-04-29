using GeneralsZeroHourEditor.AppMain;
using GeneralsZeroHourEditor.UX.ViewModels.HomePage;
using Microsoft.Extensions.DependencyInjection;

namespace GeneralsZeroHourEditor.UX.Views.HomePage;

public sealed partial class HomePage
{
    public HomePageViewModel ViewModel { get; }

    public HomePage()
    {
        InitializeComponent();
        ViewModel = App.Services!.GetRequiredService<HomePageViewModel>();
        DataContext = ViewModel;

        Loaded += (_, _) =>
        {
            ViewModel.NavigationService.TopFrame = TopBarFrame;
            ViewModel.NavigationService.ActiveFrame = ActiveFrame;
        };
    }
}
