using System.Reflection;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;

namespace GeneralsZeroHourEditor.Behaviors;

public class PointerCursorBehavior
{
    public static readonly DependencyProperty CursorTypeProperty = DependencyProperty.RegisterAttached(
        "CursorType", typeof(InputSystemCursorShape?), typeof(PointerCursorBehavior), new PropertyMetadata(null, OnCursorTypeChanged));

    public static void SetCursorType(DependencyObject element, InputSystemCursorShape? value)
    {
        element.SetValue(CursorTypeProperty, value);
    }

    public static InputSystemCursorShape? GetCursorType(DependencyObject element)
    {
        return (InputSystemCursorShape?) element.GetValue(CursorTypeProperty);
    }

    private static void OnCursorTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not UIElement element) return;
        UpdateCursor(element);
    }

    private static void UpdateCursor(UIElement element)
    {
        var shape = GetCursorType(element);

        if (shape is { } s)
        {
            SetProtectedCursor(element, InputSystemCursor.Create(s));
        }
        else
        {
            SetProtectedCursor(element, null);
        }
    }

    private static void SetProtectedCursor(UIElement element, InputCursor? cursor)
    {
        try
        {
            var prop = typeof(UIElement).GetProperty("ProtectedCursor", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            prop?.SetValue(element, cursor);
        }
        catch
        {
            // Ignore errors
        }
    }
}
