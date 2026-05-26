using System.Collections.ObjectModel;

namespace GeneralsZeroHourEditor.Models;

public class GameObjectModuleModel : BaseModel
{
    public string? Name
    {
        get;
        set => SetField(ref field, value);
    }

    public ObservableCollection<string> Fields { get; } = [];

    public ObservableCollection<GameObjectSubBlockModel> SubBlocks { get; } = [];
}

public class GameObjectSubBlockModel : BaseModel
{
    public string? Name
    {
        get;
        set => SetField(ref field, value);
    }

    public ObservableCollection<string> Fields { get; } = [];
}

public abstract class GameObjectNodeModel : BaseModel
{
    public string Name
    {
        get;
        set => SetField(ref field, value);
    }

    public abstract ObservableCollection<GameObjectItemModel>? Children { get; }
}

public class GameObjectItemModel : GameObjectNodeModel
{
    public override ObservableCollection<GameObjectItemModel>? Children => null;
}

public class SideGroupModel : GameObjectNodeModel
{
    public override ObservableCollection<GameObjectItemModel> Children { get; } = [];
}

public class GameObjectPageModel : BaseModel
{
    public ObservableCollection<SideGroupModel> GameObjectGroups { get; } = [];

    public object? SelectedNode
    {
        get;
        set => SetField(ref field, value);
    }

    public GameObjectItemModel? SelectedGameObject => SelectedNode as GameObjectItemModel;

    public ObservableCollection<GameObjectModuleModel> Modules { get; } = [];

    public GameObjectDetailModel? Detail
    {
        get;
        set => SetField(ref field, value);
    }
}
