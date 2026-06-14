using System.Collections.ObjectModel;
using GeneralsZeroHourEditor.Enumerations;
using MercuryLibrary.WinUI3Components;

namespace GeneralsZeroHourEditor.Models.WeaponSet;

public class WeaponAutoChooseSourceModel : PropertyChangedUpdater
{
    public WeaponSlot WeaponSlot
    {
        get;
        set => SetField(ref field, value);
    } = WeaponSlot.PRIMARY;

    public ObservableCollection<AutoChooseSources> AutoChooseSources { get; } = [];
}
