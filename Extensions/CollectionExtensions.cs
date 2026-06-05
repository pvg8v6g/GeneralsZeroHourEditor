using System.Collections.ObjectModel;

namespace GeneralsZeroHourEditor.Extensions;

/// <summary>
/// Extension methods for working with <see cref="ObservableCollection{T}"/>.
/// These helpers provide convenient bulk operations commonly needed in MVVM scenarios
/// where <see cref="ObservableCollection{T}"/> is used for UI-bound collections.
/// </summary>
public static partial class Extensions
{
    #region ObservableCollection extensions

    /// <summary>
    /// Adds all <paramref name="items"/> to the target <paramref name="collection"/>.
    /// </summary>
    /// <typeparam name="T">The item type contained in the collection.</typeparam>
    /// <param name="collection">The target collection to add items to.</param>
    /// <param name="items">The items to add.</param>
    /// <remarks>
    /// This method calls <see cref="ObservableCollection{T}.Add(T)"/> for each item.
    /// Note that, by default, <see cref="ObservableCollection{T}"/> raises a change notification
    /// per added item. If you need a single notification for the whole batch, consider using a
    /// collection type that supports range operations with consolidated notifications or wrap updates
    /// with view refresh suppression at the UI layer.
    /// </remarks>
    public static void AddRange<T>(this ObservableCollection<T> collection, IEnumerable<T> items)
    {
        items.ToList().ForEach(collection.Add);
    }

    /// <summary>
    /// Removes all matching <paramref name="items"/> from the target <paramref name="collection"/>.
    /// </summary>
    /// <typeparam name="T">The item type contained in the collection.</typeparam>
    /// <param name="collection">The target collection to remove items from.</param>
    /// <param name="items">The items to remove. Only items present in the collection are removed.</param>
    /// <remarks>
    /// This method calls <see cref="ObservableCollection{T}.Remove(T)"/> for each item.
    /// If an item is not present in the collection, it is skipped.
    /// Like <see cref="AddRange"/>, this will raise change notifications per removed item.
    /// </remarks>
    public static void RemoveRange<T>(this ObservableCollection<T> collection, IEnumerable<T> items)
    {
        items.ToList().ForEach(x => collection.Remove(x));
    }

    /// <summary>
    /// Replaces the entire contents of <paramref name="collection"/> with <paramref name="items"/>.
    /// </summary>
    /// <typeparam name="T">The item type contained in the collection.</typeparam>
    /// <param name="collection">The target collection to update.</param>
    /// <param name="items">The new set of items to populate the collection with.</param>
    /// <remarks>
    /// This method clears the collection and then adds the provided items using <see cref="AddRange"/>.
    /// As a result, observers will typically receive a reset notification from <see cref="ObservableCollection{T}.Clear()"/>
    /// followed by individual add notifications for each item.
    /// </remarks>
    public static void SetRange<T>(this ObservableCollection<T> collection, IEnumerable<T> items)
    {
        collection.Clear();
        collection.AddRange(items);
    }

    #endregion
}
