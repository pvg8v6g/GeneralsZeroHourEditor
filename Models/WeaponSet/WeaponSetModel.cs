using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using GeneralsZeroHourEditor.Command;
using GeneralsZeroHourEditor.Enumerations;
using GeneralsZeroHourEditor.Extensions;

namespace GeneralsZeroHourEditor.Models.WeaponSet;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public class WeaponSlotModel : BaseModel
{
    #region Properties

    public ObservableCollection<WeaponConditions> Conditions { get; } = [];

    public ObservableCollection<WeaponModel> Weapons { get; } = [];

    public ObservableCollection<WeaponAutoChooseSourceModel> AutoChooseSources { get; } = [];

    public WeaponLockSharedAcrossSets WeaponLockSharedAcrossSets
    {
        get;
        set => SetField(ref field, value);
    } = WeaponLockSharedAcrossSets.No;

    #endregion

    #region Commands

    public RelayCommand<WeaponConditions> AddWeaponConditionCommand => new(AddWeaponCondition);

    public RelayCommand<WeaponConditions> RemoveWeaponConditionCommand => new(RemoveWeaponCondition);

    public RelayCommand<WeaponSlot> AddWeaponCommand => new(AddWeapon);

    public RelayCommand<WeaponModel> RemoveWeaponCommand => new(RemoveWeapon);

    public RelayCommand<WeaponSlot> AddAutoChooseSourcesCommand => new(AddAutoChooseSources);

    public RelayCommand<WeaponAutoChooseSourceModel> RemoveAutoChooseSourcesCommand =>
        new(RemoveAutoChooseSources);

    #endregion

    #region Actions

    private void AddWeaponCondition(WeaponConditions value)
    {
        Conditions.GuardedAdd(value);
    }

    private void RemoveWeaponCondition(WeaponConditions value)
    {
        Conditions.Remove(value);
    }

    private void AddWeapon(WeaponSlot slot)
    {
        Weapons.Add(new WeaponModel { WeaponSlot = slot, Weapon = string.Empty });
    }

    private void RemoveWeapon(WeaponModel model)
    {
        Weapons.Remove(model);
    }

    private void AddAutoChooseSources(WeaponSlot weaponSlot)
    {
        AutoChooseSources.Add(new WeaponAutoChooseSourceModel { WeaponSlot = weaponSlot });
    }

    private void RemoveAutoChooseSources(WeaponAutoChooseSourceModel model)
    {
        AutoChooseSources.Remove(model);
    }

    #endregion
}
