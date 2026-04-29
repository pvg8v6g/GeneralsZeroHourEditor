using System.Windows.Input;
using GeneralsZeroHourEditor.UX.Components.EngineRadioIcon;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;

namespace GeneralsZeroHourEditor.UX.Components.EngineButtonIcon;

public sealed partial class EngineButtonIcon
{
    public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(
        nameof(Command), typeof(ICommand), typeof(EngineButtonIcon), new PropertyMetadata(default(ICommand)));

    public ICommand? Command
    {
        get => (ICommand?) GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    public EngineButtonIcon()
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
