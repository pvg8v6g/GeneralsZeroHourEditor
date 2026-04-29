using System.Reflection;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;

namespace GeneralsZeroHourEditor.Behaviors;

public class PointerCursorBehavior
{
    public static readonly DependencyProperty CursorTypeProperty = DependencyProperty.RegisterAttached(
        "CursorType", typeof(InputSystemCursorShape?), typeof(PointerCursorBehavior), new PropertyMetadata(null, OnCursorTypeChanged));

    public static readonly DependencyProperty PointerOverCursorTypeProperty = DependencyProperty.RegisterAttached(
        "PointerOverCursorType", typeof(InputSystemCursorShape?), typeof(PointerCursorBehavior),
        new PropertyMetadata(null, OnPointerOverCursorTypeChanged));

    public static void SetCursorType(DependencyObject element, InputSystemCursorShape? value)
    {
        element.SetValue(CursorTypeProperty, value);
    }

    public static InputSystemCursorShape? GetCursorType(DependencyObject element)
    {
        return (InputSystemCursorShape?) element.GetValue(CursorTypeProperty);
    }

    public static void SetPointerOverCursorType(DependencyObject element, InputSystemCursorShape? value)
    {
        element.SetValue(PointerOverCursorTypeProperty, value);
    }

    public static InputSystemCursorShape? GetPointerOverCursorType(DependencyObject element)
    {
        return (InputSystemCursorShape?) element.GetValue(PointerOverCursorTypeProperty);
    }

    private static void OnCursorTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not UIElement element) return;
        UpdateCursor(element);
    }

    private static void OnPointerOverCursorTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not UIElement element) return;

        element.PointerEntered -= OnPointerEntered;
        element.PointerExited -= OnPointerExited;

        if (e.NewValue is not null)
        {
            element.PointerEntered += OnPointerEntered;
            element.PointerExited += OnPointerExited;
        }

        UpdateCursor(element);
    }

    private static void OnPointerEntered(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        if (sender is UIElement element)
        {
            UpdateCursor(element, true);
        }
    }

    private static void OnPointerExited(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        if (sender is UIElement element)
        {
            UpdateCursor(element, false);
        }
    }

    private static void UpdateCursor(UIElement element, bool isPointerOver = false)
    {
        var shape = isPointerOver ? GetPointerOverCursorType(element) : GetCursorType(element);
        if (shape == null && isPointerOver)
        {
            shape = GetCursorType(element);
        }

        if (shape is InputSystemCursorShape s)
        {
            SetProtectedCursor(element, InputSystemCursor.Create(s));
        }
        else
        {
            SetProtectedCursor(element, null);
        }
    }

    private static void SetProtectedCursor(UIElement element, InputCursor cursor)
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
