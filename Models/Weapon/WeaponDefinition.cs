using System.Collections.ObjectModel;
using GeneralsZeroHourEditor.Enumerations;
using MercuryLibrary.WinUI3Components;

namespace GeneralsZeroHourEditor.Models.Weapon;

// Represents a fully parsed Weapon from Weapon.ini
// Keep strongly-typed common fields and a catch-all store for unrecognized properties.
public class WeaponDefinition : PropertyChangedUpdater
{
    #region Top-level

    public string Name
    {
        get;
        set => SetField(ref field, value);
    } = string.Empty;

    // No catch-all properties. Every supported INI key must have a field below.

    #endregion

    #region Common weapon fields (popular keys)

    public string PrimaryDamage
    {
        get;
        set => SetField(ref field, value);
    } = string.Empty;

    public string PrimaryDamageRadius
    {
        get;
        set => SetField(ref field, value);
    } = string.Empty;

    public string AttackRange
    {
        get;
        set => SetField(ref field, value);
    } = string.Empty;

    public string MinimumAttackRange
    {
        get;
        set => SetField(ref field, value);
    } = string.Empty;

    public string PreAttackDelay
    {
        get;
        set => SetField(ref field, value);
    } = string.Empty;

    public string DelayBetweenShots
    {
        get;
        set => SetField(ref field, value);
    } = string.Empty;

    public string ClipSize
    {
        get;
        set => SetField(ref field, value);
    } = string.Empty;

    public string ClipReloadTime
    {
        get;
        set => SetField(ref field, value);
    } = string.Empty;

    public string ProjectileObject
    {
        get;
        set => SetField(ref field, value);
    } = string.Empty;

    public string FireFX
    {
        get;
        set => SetField(ref field, value);
    } = string.Empty;

    public string RadiusDamageAffects
    {
        get;
        set => SetField(ref field, value);
    } = string.Empty;

    public string ScatterRadius
    {
        get;
        set => SetField(ref field, value);
    } = string.Empty;

    public string WeaponSpeed
    {
        get;
        set => SetField(ref field, value);
    } = string.Empty;

    public DamageType DamageType
    {
        get;
        set => SetField(ref field, value);
    }

    public DeathType DeathType
    {
        get;
        set => SetField(ref field, value);
    }

    public string ProjectileDetonationOCL
    {
        get;
        set => SetField(ref field, value);
    } = string.Empty;

    public ProjectileCollidesWith ProjectileCollidesWith
    {
        get;
        set => SetField(ref field, value);
    }

    public string AcceptableAimDelta
    {
        get;
        set => SetField(ref field, value);
    } = string.Empty;

    public string WeaponBonusDamageScalar
    {
        get;
        set => SetField(ref field, value);
    } = string.Empty;

    public string Meta
    {
        get;
        set => SetField(ref field, value);
    } = string.Empty;

    public string Report
    {
        get;
        set => SetField(ref field, value);
    } = string.Empty;

    #endregion

}
