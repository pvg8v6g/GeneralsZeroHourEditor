using GeneralsZeroHourEditor.AppMain;
using GeneralsZeroHourEditor.UX.ViewModels.WeaponPage;
using Microsoft.Extensions.DependencyInjection;

namespace GeneralsZeroHourEditor.UX.Views.WeaponPage;

public sealed partial class WeaponPage
{
    public WeaponPageViewModel ViewModel { get; }

    public WeaponPage()
    {
        InitializeComponent();
        ViewModel = App.Services!.GetRequiredService<WeaponPageViewModel>();
        DataContext = ViewModel;
    }
}
