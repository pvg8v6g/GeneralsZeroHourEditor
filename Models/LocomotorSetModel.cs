namespace GeneralsZeroHourEditor.Models;

public class LocomotorSetModel : BaseModel
{
    public string Locomotor
    {
        get;
        set => SetField(ref field, value);
    } = string.Empty;

    public string ConditionsCsv
    {
        get;
        set => SetField(ref field, value);
    } = "SET_NORMAL";
}
