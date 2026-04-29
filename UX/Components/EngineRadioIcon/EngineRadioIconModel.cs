using System.Collections.ObjectModel;
using GeneralsZeroHourEditor.Models;
using MercuryLibrary.WinUI3Components;

namespace GeneralsZeroHourEditor.UX.Components.EngineRadioIcon;

public partial class EngineRadioIconModel : PropertyChangedUpdater
{
    public CroppedImage? CroppedImage
    {
        get;
        set => SetField(ref field, value);
    }

    public bool IsChecked
    {
        get;
        set => SetField(ref field, value);
    }

    public string CommandIndex
    {
        get;
        set => SetField(ref field, value);
    } = "0";

    public string Tooltip
    {
        get;
        set => SetField(ref field, value);
    } = string.Empty;

    public ObservableCollection<MenuItemModel> MenuItems
    {
        get;
        set => SetField(ref field, value);
    } = [];
}

public partial class MenuItemModel : PropertyChangedUpdater
{
    public string Header
    {
        get;
        set => SetField(ref field, value);
    } = string.Empty;

    public string CommandIndex
    {
        get;
        set => SetField(ref field, value);
    } = string.Empty;
}
