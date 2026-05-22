using GeneralsZeroHourEditor.AppMain;
using GeneralsZeroHourEditor.UX.ViewModels.VehiclePage;
using Microsoft.Extensions.DependencyInjection;

namespace GeneralsZeroHourEditor.UX.Views.VehiclePage;

public partial class VehiclePage
{
    public VehiclePageViewModel ViewModel { get; }

    public VehiclePage()
    {
        InitializeComponent();
        ViewModel = App.Services!.GetRequiredService<VehiclePageViewModel>();
        DataContext = ViewModel;
    }
}
