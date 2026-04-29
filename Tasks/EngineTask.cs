using MercuryLibrary.WinUI3Components;

namespace GeneralsZeroHourEditor.Tasks;

public abstract class EngineTask : PropertyChangedUpdater
{
    #region Properties

    public int Work
    {
        get;
        set
        {
            field = value;
            Progress = (int) (100 * ((decimal) value / (decimal) MaxWork));
        }
    }

    protected int MaxWork { get; set; }

    public int Progress
    {
        get;
        private set => SetField(ref field, value);
    }

    #endregion

    #region Functions

    public abstract Task Call();

    protected async Task CreateWorkTask(Task task)
    {
        await task;
        Work++;
    }

    protected async Task<T> CreateWorkTask<T>(Task<T> task)
    {
        var t = await task;
        Work++;
        return t;
    }

    #endregion
}
