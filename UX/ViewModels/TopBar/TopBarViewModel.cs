using System.Collections.ObjectModel;
using GeneralsZeroHourEditor.Command;
using GeneralsZeroHourEditor.Services.GraphicsService;
using GeneralsZeroHourEditor.Services.NavigationService;
using GeneralsZeroHourEditor.Tasks;
using GeneralsZeroHourEditor.UX.Components.EngineRadioIcon;

namespace GeneralsZeroHourEditor.UX.ViewModels.TopBar;

public class TopBarViewModel(INavigationService navigationService, IGraphicsService graphicsService) : BaseViewModel
{
    #region Properties

    public ObservableCollection<EngineRadioIconModel> EngineImages
    {
        get;
        set => SetField(ref field, value);
    } = [];

    public EngineRadioIconModel? MediaImage
    {
        get;
        set => SetField(ref field, value);
    }

    public EngineRadioIconModel? SaveImage
    {
        get;
        set => SetField(ref field, value);
    }

    #endregion

    #region Actions

    protected override async Task LoadedAction()
    {
        SaveImage = new EngineRadioIconModel
        {
            CroppedImage = await graphicsService.GetEngineIcon(11),
            CommandIndex = "12",
            Tooltip = "Save"
        };

        EngineRadioIconModel[] buttonIcons =
        [
            new()
            {
                IsChecked = true,
                CommandIndex = "0",
                Tooltip = "Infantry"
            },
            new()
            {
                IsChecked = false,
                CommandIndex = "1",
                Tooltip = "Vehicles"
            },
            new()
            {
                IsChecked = false,
                CommandIndex = "2",
                Tooltip = "Armors"
            },
            new()
            {
                IsChecked = false,
                CommandIndex = "3",
                Tooltip = "Locomotors"
            },
            new()
            {
                IsChecked = false,
                CommandIndex = "4",
                Tooltip = "Science"
            },
            new()
            {
                IsChecked = false,
                CommandIndex = "5",
                Tooltip = "Upgrades"
            },
            new()
            {
                IsChecked = false,
                CommandIndex = "6",
                Tooltip = "Weapons"
            },
            new()
            {
                IsChecked = false,
                CommandIndex = "7",
                Tooltip = "Special Powers"
            },
            new()
            {
                IsChecked = false,
                CommandIndex = "8",
                Tooltip = "Command Buttons"
            },
            new()
            {
                IsChecked = false,
                CommandIndex = "9",
                Tooltip = "Command Sets"
            },
            new()
            {
                IsChecked = false,
                CommandIndex = "9",
                Tooltip = "Object Creation"
            },
        ];

        var tasks = buttonIcons
            .Select(async (x, i) =>
            {
                x.CroppedImage = await graphicsService.GetEngineIcon(i);
                return x;
            })
            .ToArray();

        EngineImages = new ObservableCollection<EngineRadioIconModel>(await Task.WhenAll(tasks));
    }

    public RelayCommand<string> TopBarCommand => new(TopBarAction);

    private void TopBarAction(string index)
    {
        switch (index)
        {
            case "0": // infantry
                navigationService.NavigateTo<Views.InfantryPage.InfantryPage>();
                break;
            case "1": // vehicle
                navigationService.NavigateTo<Views.VehiclePage.VehiclePage>();
                break;
            case "2":
                break;
            case "3":
                break;
            case "4":
                break;
            case "5":
                break;
            case "6":
                break;
            case "7":
                break;
            case "8":
                break;
            case "9":
                break;
            case "10":
                break;
            case "11": // saving
                navigationService.ShowProgressPopup<SaveProjectDataTask>("Saving Game Files");
                break;
        }
    }

    #endregion
}
