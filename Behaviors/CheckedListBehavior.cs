using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Windows.Foundation;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace GeneralsZeroHourEditor.Behaviors;

public static class CheckedListBehavior
{
    #region Dependency Properties

    public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.RegisterAttached(
        "ItemsSource",
        typeof(IList),
        typeof(CheckedListBehavior),
        new PropertyMetadata(null, OnItemsSourceChanged));

    public static readonly DependencyProperty ValueProperty = DependencyProperty.RegisterAttached(
        "Value",
        typeof(object),
        typeof(CheckedListBehavior),
        new PropertyMetadata(null, OnValueChanged));

    // Track collection-change subscriptions so we can refresh checkbox state when the
    // bound collection mutates (not just when the DP itself changes).
    private static readonly DependencyProperty SubscribedCollectionProperty = DependencyProperty.RegisterAttached(
        "SubscribedCollection",
        typeof(INotifyCollectionChanged),
        typeof(CheckedListBehavior),
        new PropertyMetadata(null));

    private static readonly DependencyProperty CollectionChangedHandlerProperty = DependencyProperty.RegisterAttached(
        "CollectionChangedHandler",
        typeof(NotifyCollectionChangedEventHandler),
        typeof(CheckedListBehavior),
        new PropertyMetadata(null));

    #endregion

    public static void SetItemsSource(DependencyObject element, IList? value)
    {
        element.SetValue(ItemsSourceProperty, value);
    }

    public static IList? GetItemsSource(DependencyObject element)
    {
        return (IList?) element.GetValue(ItemsSourceProperty);
    }

    public static void SetValue(DependencyObject element, object? value)
    {
        element.SetValue(ValueProperty, value);
    }

    public static object? GetValue(DependencyObject element)
    {
        return element.GetValue(ValueProperty);
    }

    private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not CheckBox checkBox) return;
        WireUp(checkBox);
        WireCollectionChanged(checkBox, e.OldValue as IList, e.NewValue as IList);
        // Immediate refresh plus a deferred one to cover template timing
        TryAutoBindItemsSource(checkBox);
        RefreshCheckState(checkBox);
        _ = checkBox.DispatcherQueue?.TryEnqueue(() => RefreshCheckState(checkBox));
    }

    private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not CheckBox checkBox) return;
        WireUp(checkBox);
        // Immediate refresh plus a deferred one to cover template timing
        TryAutoBindItemsSource(checkBox);
        RefreshCheckState(checkBox);
        _ = checkBox.DispatcherQueue?.TryEnqueue(() => RefreshCheckState(checkBox));
    }

    private static void WireUp(CheckBox checkBox)
    {
        // Ensure we don't double-subscribe
        checkBox.Checked -= OnChecked;
        checkBox.Unchecked -= OnUnchecked;
        checkBox.Checked += OnChecked;
        checkBox.Unchecked += OnUnchecked;

        // One-time post-load refresh to avoid first-item timing issues in nested templates
        RoutedEventHandler? onLoaded = null;
        onLoaded = (_, _) =>
        {
            checkBox.Loaded -= onLoaded;
            TryAutoBindItemsSource(checkBox);
            RefreshCheckState(checkBox);
            _ = checkBox.DispatcherQueue?.TryEnqueue(() => RefreshCheckState(checkBox));
        };
        checkBox.Loaded -= onLoaded;
        checkBox.Loaded += onLoaded;

        // Also react when the DataContext changes (first materialized item often suffers
        // from timing issues where ElementName/ancestor bindings aren’t ready yet)
        TypedEventHandler<FrameworkElement, DataContextChangedEventArgs>? onDataContextChanged = null;
        onDataContextChanged = (_, _) =>
        {
            checkBox.DataContextChanged -= onDataContextChanged;
            RefreshCheckState(checkBox);
            checkBox.DispatcherQueue?.TryEnqueue(() => RefreshCheckState(checkBox));
        };
        checkBox.DataContextChanged -= onDataContextChanged;
        checkBox.DataContextChanged += onDataContextChanged;

        // Also enqueue a deferred refresh right away to handle cases where Loaded timing
        // is still too early relative to DataContext/ElementName resolution
        _ = checkBox.DispatcherQueue?.TryEnqueue(() => RefreshCheckState(checkBox));
    }

    [SuppressMessage("ReSharper", "UnusedParameter.Local")]
    private static void WireCollectionChanged(CheckBox checkBox, IList? oldList, IList? newList)
    {
        // Unsubscribe previously if it was observable
        if (checkBox.GetValue(SubscribedCollectionProperty) is INotifyCollectionChanged oldObservable &&
            checkBox.GetValue(CollectionChangedHandlerProperty) is NotifyCollectionChangedEventHandler oldHandler)
        {
            oldObservable.CollectionChanged -= oldHandler;
            checkBox.ClearValue(SubscribedCollectionProperty);
            checkBox.ClearValue(CollectionChangedHandlerProperty);
        }

        switch (newList)
        {
            // Subscribe new only when non-null & observable
            case INotifyCollectionChanged observable:
            {
                var handler = new NotifyCollectionChangedEventHandler((_, __) => RefreshCheckState(checkBox));
                checkBox.SetValue(SubscribedCollectionProperty, observable);
                checkBox.SetValue(CollectionChangedHandlerProperty, handler);
                observable.CollectionChanged += handler;
                break;
            }
            // Only refresh when we have a concrete newList; avoid early null refreshes
            case null:
                return;
        }

        RefreshCheckState(checkBox);
        _ = checkBox.DispatcherQueue?.TryEnqueue(() => RefreshCheckState(checkBox));
    }

    // private static void WireCollectionChanged(CheckBox checkBox, IList? oldList, IList? newList)
    // {
    //     if (oldList is null || newList is null) return;
    //     // Unsubscribe previous
    //     if (checkBox.GetValue(SubscribedCollectionProperty) is INotifyCollectionChanged oldObservable &&
    //         checkBox.GetValue(CollectionChangedHandlerProperty) is NotifyCollectionChangedEventHandler oldHandler)
    //     {
    //         oldObservable.CollectionChanged -= oldHandler;
    //     }
    //
    //     // Subscribe new if observable
    //     if (newList is INotifyCollectionChanged observable)
    //     {
    //         var handler = new NotifyCollectionChangedEventHandler((_, _) => RefreshCheckState(checkBox));
    //         checkBox.SetValue(SubscribedCollectionProperty, observable);
    //         checkBox.SetValue(CollectionChangedHandlerProperty, handler);
    //         observable.CollectionChanged += handler;
    //     }
    //     else
    //     {
    //         checkBox.ClearValue(SubscribedCollectionProperty);
    //         checkBox.ClearValue(CollectionChangedHandlerProperty);
    //     }
    // }

    private static void OnChecked(object sender, RoutedEventArgs e)
    {
        if (sender is not CheckBox checkBox) return;
        var list = GetItemsSource(checkBox);
        var value = GetValue(checkBox);
        if (list is null || value is null) return;

        // If the checkbox represents an enum value, apply special handling for a semantic "None" option.
        // We do NOT assume underlying 0 equals None. We try to find a member named "None" first,
        // then a member named "Default"; if neither exists, we fall back to the underlying 0 value.
        if (value is Enum enumValue)
        {
            var noneValue = GetSemanticNone(enumValue);
            var isNone = EnumEquals(enumValue, noneValue);

            if (isNone)
            {
                // Selecting None clears any other selections and keeps only None
                if (list.Count > 0) list.Clear();
                if (!ContainsExactEnum(list, noneValue)) list.Add(noneValue);
                return;
            }

            // Selecting a non-None value removes None if it is present
            if (ContainsExactEnum(list, noneValue))
            {
                RemoveExactEnum(list, noneValue);
            }
        }

        if (!list.Contains(value)) list.Add(value);
    }

    private static void OnUnchecked(object sender, RoutedEventArgs e)
    {
        if (sender is not CheckBox checkBox) return;
        var list = GetItemsSource(checkBox);
        var value = GetValue(checkBox);
        if (list is null || value is null) return;
        if (list.Contains(value)) list.Remove(value);

        // If unchecking resulted in an empty list and we're dealing with an enum,
        // re-add the semantic None member to represent an empty selection.
        if (list.Count == 0 && value is Enum enumValue)
        {
            var noneValue = GetSemanticNone(enumValue);
            if (!ContainsExactEnum(list, noneValue)) list.Add(noneValue);
        }
    }

    private static void RefreshCheckState(CheckBox checkBox)
    {
        var list = GetItemsSource(checkBox);
        var value = GetValue(checkBox);
        if (list is null)
        {
            checkBox.IsChecked = false;
            return;
        }

        // Enum-aware semantics: treat semantic None as checked when list is empty or explicitly contains that None value
        if (value is Enum enumValue)
        {
            var noneValue = GetSemanticNone(enumValue);
            var isNone = EnumEquals(enumValue, noneValue);
            if (isNone)
            {
                checkBox.IsChecked = list.Count == 0 || ContainsExactEnum(list, noneValue);
                return;
            }

            checkBox.IsChecked = ContainsExactEnum(list, enumValue);
            return;
        }

        checkBox.IsChecked = value is not null && list.Contains(value);
    }

    #region Helpers

    private static bool ContainsExactEnum(IList list, Enum value)
    {
        // Compare enum type and underlying integral value
        var targetType = value.GetType();
        var targetVal = Convert.ToInt64(value);
        foreach (var item in list)
        {
            if (item is not Enum e) continue;
            if (e.GetType() != targetType) continue;
            if (Convert.ToInt64(e) == targetVal) return true;
        }

        return false;
    }

    private static void RemoveExactEnum(IList list, Enum value)
    {
        var targetType = value.GetType();
        var targetVal = Convert.ToInt64(value);
        for (var i = list.Count - 1; i >= 0; i--)
        {
            if (list[i] is not Enum e) continue;
            if (e.GetType() != targetType) continue;
            if (Convert.ToInt64(e) == targetVal)
            {
                list.RemoveAt(i);
            }
        }
    }

    private static bool EnumEquals(Enum a, Enum b)
    {
        return a.GetType() == b.GetType() && Convert.ToInt64(a) == Convert.ToInt64(b);
    }

    private static Enum GetSemanticNone(Enum sample)
    {
        var type = sample.GetType();
        // Prefer a member literally named "None"
        foreach (var v in Enum.GetValues(type))
        {
            if (v is not Enum e) continue;
            if (string.Equals(Enum.GetName(type, e) ?? string.Empty, "None", StringComparison.OrdinalIgnoreCase))
            {
                return e;
            }
        }

        // Then prefer a member named "Default"
        foreach (var v in Enum.GetValues(type))
        {
            if (v is not Enum e) continue;
            if (string.Equals(Enum.GetName(type, e) ?? string.Empty, "Default", StringComparison.OrdinalIgnoreCase))
            {
                return e;
            }
        }

        // Fallback: underlying 0 constant (even if it has a different name)
        return (Enum) Enum.ToObject(type, 0);
    }

    #endregion

    #region Auto-binding resolution

    private static void TryAutoBindItemsSource(CheckBox checkBox)
    {
        // If ItemsSource is already provided by XAML, keep it.
        if (GetItemsSource(checkBox) is not null) return;

        // We need a Value and it must be an enum to infer a matching list.
        var value = GetValue(checkBox);
        if (value is not Enum enumValue) return;

        // Walk up the visual tree to find an ancestor DataContext exposing a suitable IList
        if (!TryResolveItemsSourceFromAncestors(checkBox, enumValue.GetType(), out var list)) return;
        SetItemsSource(checkBox, list);
        // ensure we react on changes
        WireCollectionChanged(checkBox, null, list);
    }

    private static bool TryResolveItemsSourceFromAncestors(FrameworkElement element, Type enumType, out IList? list)
    {
        list = null;
        // First pass: prefer a property literally named "Conditions" on any ancestor DataContext
        return TryFindAncestorList(element, enumType, preferPropertyName: "Conditions", out list) ||
               // Fallback: any IList on ancestor DataContext whose element type matches enumType
               TryFindAncestorList(element, enumType, preferPropertyName: null, out list);
    }

    private static bool TryFindAncestorList(FrameworkElement element, Type enumType, string? preferPropertyName, out IList? list)
    {
        list = null;
        DependencyObject? current = element;
        while (current is not null)
        {
            if (current is FrameworkElement fe)
            {
                var dc = fe.DataContext;
                if (dc is not null)
                {
                    // Inspect public instance properties
                    var props = dc.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);

                    // If a preferred property name is specified (e.g., "Conditions"), try it first
                    if (!string.IsNullOrEmpty(preferPropertyName))
                    {
                        var prop = props.FirstOrDefault(p => string.Equals(p.Name, preferPropertyName, StringComparison.Ordinal));
                        if (prop is not null && typeof(IList).IsAssignableFrom(prop.PropertyType))
                        {
                            if (prop.GetValue(dc) is IList candidate && ListElementTypeMatches(candidate, enumType))
                            {
                                list = candidate;
                                return true;
                            }
                        }
                    }

                    // Otherwise, scan all IList properties for a matching element type
                    foreach (var p in props)
                    {
                        if (!typeof(IList).IsAssignableFrom(p.PropertyType)) continue;
                        var candidate = p.GetValue(dc) as IList;
                        if (candidate is null) continue;
                        if (ListElementTypeMatches(candidate, enumType))
                        {
                            list = candidate;
                            return true;
                        }
                    }
                }
            }

            current = VisualTreeHelper.GetParent(current);
        }

        return false;
    }

    private static bool ListElementTypeMatches(IList list, Type enumType)
    {
        // Try generic IList<T> or ObservableCollection<T>
        var t = list.GetType();
        if (t.IsGenericType)
        {
            var genArg = t.GetGenericArguments().FirstOrDefault();
            if (genArg is not null && genArg == enumType) return true;
        }

        // Fallback: inspect first element if present
        if (list.Count > 0 && list[0] is { } first)
        {
            return first.GetType() == enumType;
        }

        // Unknown; be conservative
        return false;
    }

    #endregion
}
