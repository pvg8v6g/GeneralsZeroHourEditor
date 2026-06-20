using System.Collections.ObjectModel;
using System.Text.Json;
using GeneralsZeroHourEditor.Command;
using GeneralsZeroHourEditor.Extensions;
using GeneralsZeroHourEditor.Models.Weapon;
using GeneralsZeroHourEditor.Services.GameDataService;

namespace GeneralsZeroHourEditor.UX.ViewModels.WeaponPage;

public class WeaponPageViewModel(IGameDataService gameDataService) : BaseViewModel
{
    #region Properties

    public ObservableCollection<WeaponDefinition> Weapons { get; } = [];

    public WeaponDefinition? SelectedItem
    {
        get;
        set
        {
            if (Equals(field, value)) return;
            SetField(ref field, value);
            // Update context action availability when selection changes
            CopyWeaponCommand.RaiseCanExecuteChanged();
            DeleteWeaponCommand.RaiseCanExecuteChanged();
        }
    }

    public IGameDataService GameDataService => gameDataService;

    #endregion

    #region Commands

    public RelayCommand NewWeaponCommand => new(NewWeapon);

    public RelayCommand CopyWeaponCommand => new(CopyWeapon, () => SelectedItem is not null);

    public RelayCommand PasteWeaponCommand => new(PasteWeapon, () => _copiedWeapon is not null);

    public RelayCommand DeleteWeaponCommand => new(DeleteWeapon, () => SelectedItem is not null);

    #endregion

    #region Actions & Listeners

    protected override async Task LoadedAction()
    {
        RebuildWeaponList();
        await Task.CompletedTask;
    }

    #endregion

    #region Context actions

    private WeaponDefinition? _copiedWeapon;

    private void NewWeapon()
    {
        // Create with a unique name
        var baseName = "New Weapon";
        var allNames = GameDataService.GameWeapons.Select(w => w.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var uniqueName = baseName;
        var i = 1;
        while (allNames.Contains(uniqueName))
        {
            i++;
            uniqueName = $"{baseName} {i}";
        }

        var weapon = new WeaponDefinition
        {
            Name = uniqueName
        };

        // Update source of truth
        GameDataService.GameWeapons.Add(weapon);

        // Keep local view list in sorted order by Name
        InsertSorted(weapon);

        SelectedItem = weapon;

        // Update command can-execute
        CopyWeaponCommand.RaiseCanExecuteChanged();
        PasteWeaponCommand.RaiseCanExecuteChanged();
        DeleteWeaponCommand.RaiseCanExecuteChanged();
    }

    private void CopyWeapon()
    {
        if (SelectedItem is null) return;
        _copiedWeapon = SelectedItem;
        PasteWeaponCommand.RaiseCanExecuteChanged();
    }

    private void PasteWeapon()
    {
        if (_copiedWeapon is null) return;

        // Deep clone via JSON to preserve full object graph and INotify state
        var json = JsonSerializer.Serialize(_copiedWeapon);
        var clone = JsonSerializer.Deserialize<WeaponDefinition>(json);
        if (clone is null) return;

        // Ensure unique name
        var baseName = string.IsNullOrWhiteSpace(_copiedWeapon.Name) ? "Copied Weapon" : _copiedWeapon.Name;
        var allNames = GameDataService.GameWeapons.Select(w => w.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var newName = baseName;
        var i = 1;
        while (allNames.Contains(newName))
        {
            i++;
            newName = $"{baseName} Copy {i}";
        }

        clone.Name = newName;

        GameDataService.GameWeapons.Add(clone);
        InsertSorted(clone);
        SelectedItem = clone;

        CopyWeaponCommand.RaiseCanExecuteChanged();
        PasteWeaponCommand.RaiseCanExecuteChanged();
        DeleteWeaponCommand.RaiseCanExecuteChanged();
    }

    private void DeleteWeapon()
    {
        var item = SelectedItem;
        if (item is null) return;

        // Remove from source of truth
        _ = GameDataService.GameWeapons.Remove(item);
        // Remove from local view
        var idx = Weapons.IndexOf(item);
        if (idx >= 0) Weapons.RemoveAt(idx);

        // Adjust selection
        if (Weapons.Count == 0)
        {
            SelectedItem = null;
        }
        else if (idx >= Weapons.Count)
        {
            SelectedItem = Weapons[^1];
        }
        else if (idx >= 0)
        {
            SelectedItem = Weapons[idx];
        }

        CopyWeaponCommand.RaiseCanExecuteChanged();
        PasteWeaponCommand.RaiseCanExecuteChanged();
        DeleteWeaponCommand.RaiseCanExecuteChanged();
    }

    private void InsertSorted(WeaponDefinition weapon)
    {
        var insertAt = 0;
        while (insertAt < Weapons.Count && string.Compare(Weapons[insertAt].Name, weapon.Name, StringComparison.OrdinalIgnoreCase) <= 0)
        {
            insertAt++;
        }

        Weapons.Insert(insertAt, weapon);
    }

    #endregion

    #region Private Methods

    private void RebuildWeaponList()
    {
        Weapons.SetRange(GameDataService.GameWeapons.OrderBy(w => w.Name));
    }

    #endregion
}
