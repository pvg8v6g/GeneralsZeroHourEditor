using GeneralsZeroHourEditor.Enumerations;
using MercuryLibrary.WinUI3Components;

namespace GeneralsZeroHourEditor.Models.WeaponSet;

public class WeaponModel : PropertyChangedUpdater
{
    #region Properties

    public WeaponSlot WeaponSlot
    {
        get;
        set => SetField(ref field, value);
    } = WeaponSlot.PRIMARY;

    public string Weapon
    {
        get;
        set => SetField(ref field, value);
    } = string.Empty;

    #endregion
}
