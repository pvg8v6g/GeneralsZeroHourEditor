using System.Collections.ObjectModel;
using GeneralsZeroHourEditor.Models;
using GeneralsZeroHourEditor.Models.Weapon;

namespace GeneralsZeroHourEditor.Services.GameDataService;

public interface IGameDataService
{
    public double WindowWidth { get; set; }

    public double WindowHeight { get; set; }

    public ObservableCollection<string> Sides { get; }

    // Full parsed weapons loaded from pre-generated JSON (Generals + Zero Hour)
    public ObservableCollection<WeaponDefinition> GameWeapons { get; }

    public ObservableCollection<string> GameArmors { get; }

    public ObservableCollection<string> GameLocomotors { get; }

    public ObservableCollection<string> GameSciences { get; }

    public ObservableCollection<string> FXLists { get; }

    // Preloaded game objects by category (full objects kept in memory)
    public ObservableCollection<GameObjectModel> Infantry { get; }

    public ObservableCollection<GameObjectModel> Vehicles { get; }

    public ObservableCollection<GameObjectModel> Structures { get; }
}
