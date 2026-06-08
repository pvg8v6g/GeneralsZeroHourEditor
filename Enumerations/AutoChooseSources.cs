using System.Diagnostics.CodeAnalysis;

namespace GeneralsZeroHourEditor.Enumerations;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public enum AutoChooseSources
{
    NONE,
    FROM_PLAYER,
    FROM_SCRIPT,
    FROM_AI,
    DEFAULT_SWITCH_WEAPON, // unit will pick this weapon when normal logic fails
}
