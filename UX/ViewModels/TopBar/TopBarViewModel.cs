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
            CommandIndex = "13",
            Tooltip = "Save"
        };

        EngineRadioIconModel[] buttonIcons =
        [
            new()
            {
                CroppedImage = await graphicsService.GetEngineIcon(0),
                IsChecked = true,
                CommandIndex = "0",
                Tooltip = "Infantry"
            },
            new()
            {
                CroppedImage = await graphicsService.GetEngineIcon(1),
                IsChecked = false,
                CommandIndex = "1",
                Tooltip = "Vehicles"
            },
            new()
            {
                CroppedImage = await graphicsService.GetEngineIcon(12),
                IsChecked = false,
                CommandIndex = "2",
                Tooltip = "Aircraft"
            },
            new()
            {
                CroppedImage = await graphicsService.GetEngineIcon(13),
                IsChecked = false,
                CommandIndex = "3",
                Tooltip = "Structures"
            },
            new()
            {
                CroppedImage = await graphicsService.GetEngineIcon(6),
                IsChecked = false,
                CommandIndex = "4",
                Tooltip = "Weapons"
            },
            new()
            {
                CroppedImage = await graphicsService.GetEngineIcon(2),
                IsChecked = false,
                CommandIndex = "5",
                Tooltip = "Armors"
            },
            new()
            {
                CroppedImage = await graphicsService.GetEngineIcon(3),
                IsChecked = false,
                CommandIndex = "6",
                Tooltip = "Locomotors"
            },
            new()
            {
                CroppedImage = await graphicsService.GetEngineIcon(4),
                IsChecked = false,
                CommandIndex = "7",
                Tooltip = "Science"
            },
            new()
            {
                CroppedImage = await graphicsService.GetEngineIcon(5),
                IsChecked = false,
                CommandIndex = "8",
                Tooltip = "Upgrades"
            },
            new()
            {
                CroppedImage = await graphicsService.GetEngineIcon(7),
                IsChecked = false,
                CommandIndex = "9",
                Tooltip = "Special Powers"
            },
            new()
            {
                CroppedImage = await graphicsService.GetEngineIcon(8),
                IsChecked = false,
                CommandIndex = "10",
                Tooltip = "Command Buttons"
            },
            new()
            {
                CroppedImage = await graphicsService.GetEngineIcon(9),
                IsChecked = false,
                CommandIndex = "11",
                Tooltip = "Command Sets"
            },
            new()
            {
                CroppedImage = await graphicsService.GetEngineIcon(10),
                IsChecked = false,
                CommandIndex = "12",
                Tooltip = "Object Creation"
            },
        ];

        EngineImages = new ObservableCollection<EngineRadioIconModel>(buttonIcons);
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
            case "2": // aircraft
                break;
            case "3": // structures
                break;
            case "4": // weapons
                break;
            case "5": // armors
                break;
            case "6": // locomotors
                break;
            case "7": // science
                break;
            case "8": // upgrades
                break;
            case "9": // special powers
                break;
            case "10": // command buttons
                break;
            case "11": // command sets
                break;
            case "12": // object creation
                break;
            case "13": // saving
                navigationService.ShowProgressPopup<SaveProjectDataTask>("Saving Game Files");
                break;
        }
    }

    #endregion
}
