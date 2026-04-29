using GeneralsZeroHourEditor.Services.LocationService;
using GeneralsZeroHourEditor.Services.NavigationService;
using GeneralsZeroHourEditor.Tasks;
using GeneralsZeroHourEditor.UX.Views.TopBar;

namespace GeneralsZeroHourEditor.UX.ViewModels.HomePage;

public class HomePageViewModel(INavigationService navigationService, ILocationService locationService) : BaseViewModel
{
    #region Properties

    public INavigationService NavigationService => navigationService;

    #endregion

    #region Actions

    protected override async Task LoadedAction()
    {
        if (locationService.CreateProjectDirectory())
        {
            await NavigationService.ShowProgressPopup<InitialLoadDataTask>("Loading Game Files");
            await NavigationService.ShowProgressPopup<SaveProjectDataTask>("Saving Project Files");
        }
        else
        {
            await NavigationService.ShowProgressPopup<LoadProjectDataTask>("Loading Project Files");
        }

        NavigationService.SetTopBar<TopBarPage>();
        NavigationService.NavigateTo<Views.InfantryPage.InfantryPage>();
    }

    #endregion
}
