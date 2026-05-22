using GeneralsZeroHourEditor.Services.LocationService;
using GeneralsZeroHourEditor.Services.NavigationService;
using GeneralsZeroHourEditor.Tasks;
using GeneralsZeroHourEditor.UX.Views.TopBar;
using Microsoft.Extensions.DependencyInjection;

namespace GeneralsZeroHourEditor.UX.ViewModels.HomePage;

public class HomePageViewModel(INavigationService navigationService, ILocationService locationService) : BaseViewModel
{
    #region Properties

    public INavigationService NavigationService => navigationService;

    #endregion

    #region Actions

    protected override async Task LoadedAction()
    {
        // Ensure game paths are configured BEFORE showing any progress dialogs.
        var cfg = locationService.GeneralsEditorConfig;
        if (string.IsNullOrWhiteSpace(cfg.GeneralsPath) || string.IsNullOrWhiteSpace(cfg.ZeroHourPath))
        {
            var picker = AppMain.App.Services?.GetRequiredService<GeneralsZeroHourEditor.Services.FolderPickerService.IFolderPickerService>();
            var generalsDir = picker != null ? await picker.PickGeneralsFolderAsync() ?? string.Empty : string.Empty;
            var zhDir = picker != null ? await picker.PickZeroHourFolderAsync() ?? string.Empty : string.Empty;

            if (!string.IsNullOrWhiteSpace(generalsDir) && !string.IsNullOrWhiteSpace(zhDir))
            {
                var newCfg = new Models.GeneralsEditorConfig
                {
                    Location = cfg.Location,
                    GeneralsPath = generalsDir,
                    ZeroHourPath = zhDir,
                    Configured = true
                };
                locationService.SaveConfig(newCfg);
            }
            else
            {
                // User cancelled or invalid; skip heavy loading.
                return;
            }
        }

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
