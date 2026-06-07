using System.Collections.ObjectModel;
using GeneralsZeroHourEditor.Enumerations;

namespace GeneralsZeroHourEditor.Models;

public class ArmorSetModel : BaseModel
{
    public ObservableCollection<ArmorConditions> Conditions { get; } = [];

    public string Armor
    {
        get;
        set => SetField(ref field, value);
    } = string.Empty;
}
