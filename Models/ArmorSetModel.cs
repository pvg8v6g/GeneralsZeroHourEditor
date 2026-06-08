using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using GeneralsZeroHourEditor.Enumerations;

namespace GeneralsZeroHourEditor.Models;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public class ArmorSetModel : BaseModel
{
    public ObservableCollection<ArmorConditions> Conditions { get; } = [];

    public string Armor
    {
        get;
        set => SetField(ref field, value);
    } = string.Empty;

    public string DamageFX
    {
        get;
        set => SetField(ref field, value);
    } = string.Empty;
}
