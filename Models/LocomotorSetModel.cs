using GeneralsZeroHourEditor.Enumerations;
using MercuryLibrary.WinUI3Components;

namespace GeneralsZeroHourEditor.Models;

public class LocomotorSetModel : PropertyChangedUpdater
{
    public LocomotorConditions Condition
    {
        get;
        set => SetField(ref field, value);
    } = LocomotorConditions.SET_NORMAL;

    public string Locomotor
    {
        get;
        set => SetField(ref field, value);
    } = string.Empty;
}
