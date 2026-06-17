using GeneralsZeroHourEditor.Models.Weapon;

namespace GeneralsZeroHourEditor.Services.WeaponService;

public interface IWeaponService
{
    Task<IReadOnlyList<WeaponDefinition>> LoadWeaponsAsync(string gameRootDir);
}
