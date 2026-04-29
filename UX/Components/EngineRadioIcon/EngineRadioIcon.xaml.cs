using System.Windows.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;

namespace GeneralsZeroHourEditor.UX.Components.EngineRadioIcon;

public sealed partial class EngineRadioIcon
{
    #region Registered Properties

    public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(
        nameof(Command), typeof(ICommand), typeof(EngineRadioIcon), new PropertyMetadata(null));

    public ICommand? Command
    {
        get => (ICommand?) GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    public static readonly DependencyProperty GroupNameProperty = DependencyProperty.Register(
        nameof(GroupName), typeof(string), typeof(EngineRadioIcon), new PropertyMetadata(null));

    public string? GroupName
    {
        get => (string?) GetValue(GroupNameProperty);
        set => SetValue(GroupNameProperty, value);
    }

    #endregion

    public EngineRadioIcon()
    {
        InitializeComponent();
    }

    private void OnPointerEntered(object sender, PointerRoutedEventArgs e)
    {
        if (DataContext is EngineRadioIconModel { MenuItems.Count: > 0 } model && sender is FrameworkElement element)
        {
            var flyout = FlyoutBase.GetAttachedFlyout(element) as MenuFlyout;
            if (flyout != null)
            {
                flyout.Placement = FlyoutPlacementMode.Bottom;
                flyout.Items.Clear();
                foreach (var menuItem in model.MenuItems)
                {
                    var flyoutItem = new MenuFlyoutItem
                    {
                        Text = menuItem.Header,
                        Command = Command,
                        CommandParameter = menuItem.CommandIndex
                    };
                    flyout.Items.Add(flyoutItem);
                }
            }

            FlyoutBase.ShowAttachedFlyout(element);
        }
    }
}
