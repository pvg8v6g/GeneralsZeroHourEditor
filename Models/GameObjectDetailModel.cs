using System.Collections.ObjectModel;

namespace GeneralsZeroHourEditor.Models;

public class GameObjectDetailModel : BaseModel
{
    // Identity / Metadata
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
    public ObservableCollection<PrerequisiteEntryModel> PrereqEntries { get; } = [];

    // Flags
    public ObservableCollection<string> KindOf { get; } = [];

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

    // Geometry & Shadow
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

    // Weapons
    public ObservableCollection<WeaponSetModel> WeaponSets { get; } = [];

    public ObservableCollection<LocomotorSetModel> LocomotorSets { get; } = [];

    // Available templates (populated from project JSON registries)
    public ObservableCollection<string> AvailableArmorTemplates { get; } = [];

    public ObservableCollection<string> AvailableWeaponTemplates { get; } = [];

    public ObservableCollection<string> AvailableLocomotors { get; } = [];

    // Prerequisite catalogs
    public ObservableCollection<string> AvailableObjects { get; } = [];
    public ObservableCollection<string> AvailableSciences { get; } = [];

    // Derived / context
    public string? SourceFilePath
    {
        get;
        set => SetField(ref field, value);
    }
}
