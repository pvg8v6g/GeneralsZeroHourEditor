using System.Collections.ObjectModel;
using GeneralsZeroHourEditor.Command;
using GeneralsZeroHourEditor.Models;
using GeneralsZeroHourEditor.Services.JsonService;
using MercuryLibrary.WinUI3Components;

namespace GeneralsZeroHourEditor.UX.ViewModels;

public abstract class BaseViewModel<T>(IJsonService jsonService) : PropertyChangedUpdater where T : BaseModel, new()
{
    #region Properties

    public T? CopiedModel
    {
        get;
        set => SetField(ref field, value);
    }

    public int SelectedIndex
    {
        get;
        set
        {
            if (field == value) return;
            SetField(ref field, value);
            _ = OnSelectedIndexChanged(value);
            OnPropertyChanged(nameof(SelectedEntity));
        }
    } = -1;

    public T? SelectedEntity =>
        SelectedIndex < 0 || SelectedIndex >= EntityCollection.Count || EntityCollection.Count < 1
            ? null
            : EntityCollection[SelectedIndex];

    protected abstract ObservableCollection<T> EntityCollection { get; }

    #endregion

    #region Abstractions

    protected virtual Task OnSelectedIndexChanged(int index)
    {
        return Task.CompletedTask;
    }

    #endregion

    #region Relays

    public RelayCommand<string?> NewEntityCommand => new(NewEntity);

    public RelayCommand CopyEntityCommand => new(CopyEntity);

    public RelayCommand PasteEntityCommand => new(PasteEntity);

    public RelayCommand DeleteEntityCommand => new(DeleteEntity);

    #endregion

    #region Actions

    public AsyncRelayCommand LoadedCommand => new(LoadedAction);

    protected virtual Task LoadedAction()
    {
        return Task.CompletedTask;
    }

    protected virtual void NewEntity(string? newEntityName = null)
    {
        // newEntityName ??= $"New {typeof(T).Name}";
        // var newObject = new T
        // {
        //     Id = Calculations.GetNextId(EntityCollection.Select(x => x.Id).ToArray()),
        //     Name = newEntityName
        // };
        //
        // var index = EntityCollection.AddSorted(newObject, x => x.Id);
        // SelectedIndex = index;
    }

    protected virtual void CopyEntity()
    {
        CopiedModel = EntityCollection[SelectedIndex];
    }

    protected virtual void PasteEntity()
    {
        // var clone = jsonService.Clone(CopiedModel!);
        // if (clone is null) return;
        //
        // clone.Guid = Guid.NewGuid();
        // clone.Id = Calculations.GetNextId(EntityCollection.Select(x => x.Id).ToArray());
        //
        // var index = EntityCollection.AddSorted(clone, x => x.Id);
        // SelectedIndex = index;
    }

    protected virtual void DeleteEntity()
    {
        // var index = SelectedIndex;
        // if (index < 0 || index >= EntityCollection.Count) return;
        //
        // EntityCollection.RemoveAt(index);
        //
        // if (EntityCollection.Count == 0)
        // {
        //     SelectedIndex = -1;
        // }
        // else if (index >= EntityCollection.Count)
        // {
        //     SelectedIndex = EntityCollection.Count - 1;
        // }
        // else
        // {
        //     SelectedIndex = index;
        //     // Notify SelectedEntity since the object at this index has changed
        //     OnPropertyChanged(nameof(SelectedEntity));
        // }
    }

    #endregion
}

public abstract class BaseViewModel : PropertyChangedUpdater
{
    #region Relays

    public AsyncRelayCommand LoadedCommand => new(LoadedAction);

    #endregion

    #region Actions

    protected virtual Task LoadedAction()
    {
        return Task.CompletedTask;
    }

    #endregion
}
