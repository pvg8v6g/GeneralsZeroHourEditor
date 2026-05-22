using System.Collections.ObjectModel;
using GeneralsZeroHourEditor.Models;

namespace GeneralsZeroHourEditor.Models.Schema;

public class GameRegistryModel : BaseModel
{
    public ObservableCollection<string> Weapons { get; } = new();
    public ObservableCollection<string> Armors { get; } = new();
    public ObservableCollection<string> FXLists { get; } = new();
    public ObservableCollection<string> Locomotors { get; } = new();

    public ObservableCollection<ModuleDefinitionModel> ModuleDefinitions { get; } = new();
}

public class ModuleDefinitionModel : BaseModel
{
    public string Name { get; set => SetField(ref field, value); }
    public ObservableCollection<string> Fields { get; } = new();
    public ObservableCollection<string> Variants { get; } = new();
    public ObservableCollection<SubBlockDefinitionModel> SubBlocks { get; } = new();
}

public class SubBlockDefinitionModel : BaseModel
{
    public string Name { get; set => SetField(ref field, value); }
    public ObservableCollection<string> Fields { get; } = new();
}
