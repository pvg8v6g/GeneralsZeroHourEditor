using System.Collections.ObjectModel;
using GeneralsZeroHourEditor.Models;

namespace GeneralsZeroHourEditor.Models.Schema;

public class GameRegistryModel : BaseModel
{
    public ObservableCollection<ModuleDefinitionModel> ModuleDefinitions { get; } = [];
}

public class ModuleDefinitionModel : BaseModel
{
    public string Name { get; set => SetField(ref field, value); }
    public ObservableCollection<string> Fields { get; } = [];
    public ObservableCollection<string> Variants { get; } = [];
    public ObservableCollection<SubBlockDefinitionModel> SubBlocks { get; } = [];
}

public class SubBlockDefinitionModel : BaseModel
{
    public string Name { get; set => SetField(ref field, value); }
    public ObservableCollection<string> Fields { get; } = [];
}
