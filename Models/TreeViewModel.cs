using System.Collections.ObjectModel;
using MercuryLibrary.WinUI3Components;

namespace GeneralsZeroHourEditor.Models;

public class TreeViewModel : PropertyChangedUpdater
{
    public string Name
    {
        get;
        set => SetField(ref field, value);
    } = string.Empty;

    public GameObjectModel? GameObject
    {
        get;
        set => SetField(ref field, value);
    }

    public ObservableCollection<TreeViewModel> Children { get; } = [];
}
