using System.Windows.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace GeneralsZeroHourEditor.Behaviors;

public class TreeViewBehavior
{
    public static readonly DependencyProperty ItemInvokedCommandProperty = DependencyProperty.RegisterAttached(
        "ItemInvokedCommand", typeof(ICommand), typeof(TreeViewBehavior), new PropertyMetadata(null, OnItemInvokedCommandChanged));

    private static void OnItemInvokedCommandChanged(DependencyObject depObj, DependencyPropertyChangedEventArgs e)
    {
        if (depObj is TreeView treeView && e.NewValue is ICommand command)
        {
            treeView.ItemInvoked += (s, args) =>
            {
                if (command.CanExecute(args.InvokedItem))
                {
                    command.Execute(args.InvokedItem);
                }
            };
        }
    }

    public static ICommand GetItemInvokedCommand(DependencyObject depObj)
    {
        return (ICommand)depObj.GetValue(ItemInvokedCommandProperty);
    }

    public static void SetItemInvokedCommand(DependencyObject depObj, ICommand value)
    {
        depObj.SetValue(ItemInvokedCommandProperty, value);
    }
}
