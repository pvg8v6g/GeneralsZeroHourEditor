using System.Collections.ObjectModel;

namespace GeneralsZeroHourEditor.Models;

public class GameObjectModuleModel : BaseModel
{
    public string? Name
    {
        get;
        set => SetField(ref field, value);
    }

    public ObservableCollection<string> Fields { get; } = new();

    public ObservableCollection<GameObjectSubBlockModel> SubBlocks { get; } = new();
}

public class GameObjectSubBlockModel : BaseModel
{
    public string? Name
    {
        get;
        set => SetField(ref field, value);
    }

    public ObservableCollection<string> Fields { get; } = new();
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
    public override ObservableCollection<GameObjectItemModel> Children { get; } = new();
}

public class GameObjectPageModel : BaseModel
{
    public ObservableCollection<SideGroupModel> GameObjectGroups { get; } = new();

    public object? SelectedNode
    {
        get;
        set => SetField(ref field, value);
    }

    public GameObjectItemModel? SelectedGameObject => SelectedNode as GameObjectItemModel;

    public ObservableCollection<GameObjectModuleModel> Modules { get; } = new();
}
