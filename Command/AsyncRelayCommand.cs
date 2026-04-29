using System.Windows.Input;

namespace GeneralsZeroHourEditor.Command;

public partial class AsyncRelayCommand(Func<Task> execute, Func<bool>? canExecute = null) : ICommand
{
    public event EventHandler? CanExecuteChanged;

    #region ICommand Members

    public bool CanExecute(object? parameter)
    {
        return canExecute == null || canExecute();
    }

    public async void Execute(object? parameter)
    {
        await execute();
    }

    #endregion

    public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}

public partial class AsyncRelayCommand<T>(Func<T, Task> execute, Func<T, bool>? canExecute = null) : ICommand
{
    public event EventHandler? CanExecuteChanged;

    public bool CanExecute(object? parameter)
    {
        if (parameter is null) return false;
        return canExecute == null || canExecute((T)parameter);
    }

    public async void Execute(object? parameter)
    {
        if (parameter is null) return;
        await execute((T)parameter);
    }

    public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}