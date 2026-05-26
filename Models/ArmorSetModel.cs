namespace GeneralsZeroHourEditor.Models;

public class ArmorSetModel : BaseModel
{
    public string Armor
    {
        get;
        set => SetField(ref field, value);
    }

    // Comma-separated list of condition tokens (e.g., PLAYER_UPGRADE, HEROIC)
    public string ConditionsCsv
    {
        get;
        set => SetField(ref field, value);
    }
}
