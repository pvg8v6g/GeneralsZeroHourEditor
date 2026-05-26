namespace GeneralsZeroHourEditor.Models;

public class WeaponSetModel : BaseModel
{
    // Comma-separated list of condition tokens controlling this set
    public string ConditionsCsv
    {
        get;
        set => SetField(ref field, value);
    }

    public string Primary
    {
        get;
        set => SetField(ref field, value);
    }

    public string Secondary
    {
        get;
        set => SetField(ref field, value);
    }

    public string Tertiary
    {
        get;
        set => SetField(ref field, value);
    }
}
