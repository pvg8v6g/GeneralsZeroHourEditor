using System.Collections.ObjectModel;
using GeneralsZeroHourEditor.Enumerations;

namespace GeneralsZeroHourEditor.Models;

public class WeaponSlotModel : BaseModel
{
    public ObservableCollection<WeaponConditions> Conditions { get; } = [];

    public ObservableCollection<KeyValuePair<WeaponSlot, string>> Weapons { get; } = [];
}
