using GeneralsZeroHourEditor.AppMain;
using GeneralsZeroHourEditor.UX.ViewModels.InfantryPage;
using Microsoft.Extensions.DependencyInjection;

namespace GeneralsZeroHourEditor.UX.Views.InfantryPage;

public sealed partial class InfantryPage
{
    public InfantryPageViewModel ViewModel { get; }

    public InfantryPage()
    {
        InitializeComponent();
        ViewModel = App.Services!.GetRequiredService<InfantryPageViewModel>();
        DataContext = ViewModel;
    }
}
