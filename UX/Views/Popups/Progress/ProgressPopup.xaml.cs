using GeneralsZeroHourEditor.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;

namespace GeneralsZeroHourEditor.UX.Views.Popups.Progress;

public partial class ProgressPopup : ContentDialog
{
    #region Properties

    public EngineTask EngineTask { get; set; } = null!;

    public string ProgressText
    {
        get => ProgressLabel.Text;
        set => ProgressLabel.Text = value;
    }

    #endregion

    public ProgressPopup()
    {
        InitializeComponent();
    }

    private void ValueOnChanged(object sender, RangeBaseValueChangedEventArgs e)
    {
        if (e.NewValue < 100) return;
        Hide();
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        var progressBinding = new Binding
        {
            Source = EngineTask,
            Path = new PropertyPath("Progress"),
            Mode = BindingMode.OneWay,
            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
        };
        BindingOperations.SetBinding(ProgressBar, RangeBase.ValueProperty, progressBinding);
        await EngineTask.Call();
    }
}
