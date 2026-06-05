using System.Collections.ObjectModel;
using MercuryLibrary.WinUI3Components;
using GeneralsZeroHourEditor.Models;

namespace GeneralsZeroHourEditor.Services.GameDataService;

public class GameDataService : PropertyChangedUpdater, IGameDataService
{
    #region Properties

    public double WindowWidth
    {
        get;
        set => SetField(ref field, value);
    } = 0.0d;

    public double WindowHeight
    {
        get;
        set => SetField(ref field, value);
    } = 0.0d;

    public ObservableCollection<string> GameWeapons { get; } = [];

    public ObservableCollection<string> GameArmors { get; } = [];

    public ObservableCollection<string> GameLocomotors { get; } = [];

    public ObservableCollection<string> FXLists { get; } = [];

    // Preloaded entities
    public ObservableCollection<GameObjectDetailModel> Infantry { get; } = [];

    public ObservableCollection<GameObjectDetailModel> Vehicles { get; } = [];

    public ObservableCollection<GameObjectDetailModel> Structures { get; } = [];

    #endregion
}
