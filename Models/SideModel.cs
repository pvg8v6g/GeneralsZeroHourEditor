namespace GeneralsZeroHourEditor.Models;

// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
public sealed class SideModel(string side, string? baseSide)
{
    #region Properties

    public string Side { get => field; set => field = value; } = side;

    public string? BaseSide { get => field; set => field = value; } = baseSide;

    #endregion
}
