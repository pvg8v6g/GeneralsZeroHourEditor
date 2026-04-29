using System.Windows.Input;
using Microsoft.UI.Xaml;

namespace GeneralsZeroHourEditor.Behaviors;

public class LoadedBehavior
{
    public static readonly DependencyProperty LoadedCommandProperty = DependencyProperty.RegisterAttached(
        "LoadedCommand", typeof(ICommand), typeof(LoadedBehavior), new PropertyMetadata(null, OnLoadedCommandChanged));

    private static void OnLoadedCommandChanged(DependencyObject depObj, DependencyPropertyChangedEventArgs e)
    {
        if (depObj is FrameworkElement frameworkElement && e.NewValue is ICommand command)
        {
            frameworkElement.Loaded += (s, args) => { command.Execute(null); };
        }
    }

    public static ICommand GetLoadedCommand(DependencyObject depObj)
    {
        return (ICommand) depObj.GetValue(LoadedCommandProperty);
    }

    public static void SetLoadedCommand(DependencyObject depObj, ICommand value)
    {
        depObj.SetValue(LoadedCommandProperty, value);
    }
}
