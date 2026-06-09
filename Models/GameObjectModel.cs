using System.Collections.ObjectModel;
using GeneralsZeroHourEditor.Enumerations;

namespace GeneralsZeroHourEditor.Models;

public class GameObjectModel : BaseModel
{
    #region Metadata

    public string Name
    {
        get;
        set => SetField(ref field, value);
    }

    public string Side
    {
        get;
        set => SetField(ref field, value);
    }

    public string EditorSorting
    {
        get;
        set => SetField(ref field, value);
    }

    public string CommandSet
    {
        get;
        set => SetField(ref field, value);
    }

    public string DisplayName
    {
        get;
        set => SetField(ref field, value);
    }

    public string Description
    {
        get;
        set => SetField(ref field, value);
    }

    // UI Art
    public string SelectPortrait
    {
        get;
        set => SetField(ref field, value);
    }

    public string ButtonImage
    {
        get;
        set => SetField(ref field, value);
    }

    public ObservableCollection<string> UpgradeCameos { get; } = [];

    // Production
    public string BuildCost
    {
        get;
        set => SetField(ref field, value);
    }

    public string BuildTime
    {
        get;
        set => SetField(ref field, value);
    }

    public string BuildCompletion
    {
        get;
        set => SetField(ref field, value);
    }

    // Structured prerequisites (Type = Object/Science, Value = name)
    public ObservableCollection<PrerequisiteSetModel> PrereqEntries { get; } = [];

    // Flags
    public ObservableCollection<KindOf> KindOf { get; } = [];

    #endregion

    #region Visuals (W3D)

    #endregion

    #region Armor & Health

    // Armor & Health
    public string MaxHealth
    {
        get;
        set => SetField(ref field, value);
    }

    public string InitialHealth
    {
        get;
        set => SetField(ref field, value);
    }

    public ObservableCollection<ArmorSetModel> ArmorSets { get; } = [];

    #endregion

    #region Weapons

    public ObservableCollection<WeaponSlotModel> WeaponSets { get; } = [];

    #endregion

    #region Behaviors / Modules

    #endregion

    #region Movement

    public ObservableCollection<KeyValuePair<LocomotorConditions, string>> LocomotorSets { get; } = [];

    #endregion

    #region Vision & Shroud

    // Vision & Shroud
    public string VisionRange
    {
        get;
        set => SetField(ref field, value);
    }

    public string ShroudClearingRange
    {
        get;
        set => SetField(ref field, value);
    }

    #endregion

    #region Geometry & Shadow

    public string Geometry
    {
        get;
        set => SetField(ref field, value);
    }

    public string GeometryMajorRadius
    {
        get;
        set => SetField(ref field, value);
    }

    public string GeometryMinorRadius
    {
        get;
        set => SetField(ref field, value);
    }

    public string GeometryHeight
    {
        get;
        set => SetField(ref field, value);
    }

    public string GeometryIsSmall
    {
        get;
        set => SetField(ref field, value);
    }

    public string Shadow
    {
        get;
        set => SetField(ref field, value);
    }

    public string ShadowSizeX
    {
        get;
        set => SetField(ref field, value);
    }

    public string ShadowSizeY
    {
        get;
        set => SetField(ref field, value);
    }

    public string ShadowTexture
    {
        get;
        set => SetField(ref field, value);
    }

    #endregion

    #region Audio & FX

    #endregion

    #region Flags & Classification

    #endregion
}
