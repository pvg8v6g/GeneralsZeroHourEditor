using System.Collections.ObjectModel;

namespace GeneralsZeroHourEditor.Models;

public class PrerequisiteEntryModel : BaseModel
{
    #region Properties

    public string Type
    {
        get;
        set
        {
            if (SetField(ref field, value))
            {
                RebuildItems();
            }
        }
    } = "Object";

    public string Value
    {
        get;
        set => SetField(ref field, value);
    } = string.Empty;

    // Bind this to the right-hand ComboBox; it is rebuilt when Type/catalogs change
    public ObservableCollection<string> Items { get; } = [];

    #endregion

    #region Fields

    private IReadOnlyList<string> _objects = [];
    private IReadOnlyList<string> _sciences = [];

    #endregion

    #region Constructor

    public void AttachCatalogs(
        IReadOnlyList<string> objects,
        IReadOnlyList<string> sciences)
    {
        _objects = objects;
        _sciences = sciences;
        RebuildItems();
    }

    #endregion

    #region Private Methods

    private void RebuildItems()
    {
        Items.Clear();
        var src = string.Equals(Type, "Science", StringComparison.OrdinalIgnoreCase) ? _sciences : _objects;
        foreach (var s in src) Items.Add(s);
        // Ensure Value is valid
        if (!string.IsNullOrWhiteSpace(Value) && Items.Contains(Value)) return;
        Value = Items.FirstOrDefault() ?? string.Empty;
    }

    #endregion
}
