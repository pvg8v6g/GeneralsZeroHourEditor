using System.Windows.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace GeneralsZeroHourEditor.Behaviors;

public class ComboBoxSelectionChangedBehavior
{
    #region Dependency Properties

    public static readonly DependencyProperty SelectionChangedCommandProperty = DependencyProperty.RegisterAttached(
        "SelectionChangedCommand",
        typeof(ICommand),
        typeof(ComboBoxSelectionChangedBehavior),
        new PropertyMetadata(null, OnSelectionChangedCommandChanged));

    #endregion

    private static void OnSelectionChangedCommandChanged(DependencyObject depObj, DependencyPropertyChangedEventArgs e)
    {
        if (depObj is not ComboBox comboBox) return;

        if (e.NewValue is ICommand command)
        {
            comboBox.SelectionChanged += (_, _) =>
            {
                var param = comboBox.SelectedItem;
                if (command.CanExecute(param))
                {
                    command.Execute(param);
                }
            };
        }
    }

    public static ICommand GetSelectionChangedCommand(DependencyObject depObj)
    {
        return (ICommand)depObj.GetValue(SelectionChangedCommandProperty);
    }

    public static void SetSelectionChangedCommand(DependencyObject depObj, ICommand value)
    {
        depObj.SetValue(SelectionChangedCommandProperty, value);
    }
}
