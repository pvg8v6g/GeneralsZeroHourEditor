using System;

namespace GeneralsZeroHourEditor.Enumerations;

// Source: SAGE Engine (Weapon.h :: enum WeaponCollideMaskType / TheWeaponCollideMaskNames[])
[Flags]
public enum ProjectileCollidesWith
{
    NONE = 0,
    ALLIES = 0x0001,
    ENEMIES = 0x0002,
    STRUCTURES = 0x0004,               // all structures EXCEPT controller's structures
    SHRUBBERY = 0x0008,
    PROJECTILES = 0x0010,
    WALLS = 0x0020,
    SMALL_MISSILES = 0x0040,           // all missiles are also projectiles
    BALLISTIC_MISSILES = 0x0080,       // all missiles are also projectiles
    CONTROLLED_STRUCTURES = 0x0100     // ONLY controller's structures
}
