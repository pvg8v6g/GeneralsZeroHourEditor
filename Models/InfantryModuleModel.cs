using System.Collections.ObjectModel;
using GeneralsZeroHourEditor.Models;

namespace GeneralEditor.UX.Models.Infantry;

public class InfantryModuleModel : BaseModel
{
    public string? Name
    {
        get;
        set => SetField(ref field, value);
    }

    public ObservableCollection<string> Fields { get; } = new();

    public ObservableCollection<InfantrySubBlockModel> SubBlocks { get; } = new();
}

public class InfantrySubBlockModel : BaseModel
{
    public string? Name
    {
        get;
        set => SetField(ref field, value);
    }

    public ObservableCollection<string> Fields { get; } = new();
}

public class InfantryPageModel : BaseModel
{
    public ObservableCollection<string> Infantry { get; } = new();

    public string? SelectedInfantry
    {
        get;
        set => SetField(ref field, value);
    }

    public ObservableCollection<InfantryModuleModel> Modules { get; } = new();
}
