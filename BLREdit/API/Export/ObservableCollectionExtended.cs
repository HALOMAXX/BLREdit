using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace BLREdit.Export;

[Serializable]
public sealed class ObservableCollectionExtended<T> : Collection<T>, INotifyCollectionChanged, INotifyPropertyChanged
{
    T fromVal;
    T toVal;
    int from;
    int to;

    public void Exchange(int From, int To)
    {
        CheckReentrancy();

        from = From;
        to = To;

        fromVal = base[from];
        toVal = base[to];

        if (to > from)
        {
            base.RemoveItem(to);
            base.RemoveItem(from);
            base.InsertItem(from, toVal);
            base.InsertItem(to, fromVal);
        }
        else
        {
            base.RemoveItem(from);
            base.RemoveItem(to);
            base.InsertItem(to, fromVal);
            base.InsertItem(from, toVal);
        }
    }

    public void SignalExchange()
    {
        OnPropertyChanged(IndexerName);

        OnCollectionChanged(NotifyCollectionChangedAction.Replace, fromVal, toVal, from);
        OnCollectionChanged(NotifyCollectionChangedAction.Replace, toVal, fromVal, to);

        //OnCollectionChanged(NotifyCollectionChangedAction.Move, fromVal, to, from);
        //OnCollectionChanged(NotifyCollectionChangedAction.Move, toVal, from, to);

        //--Move-- OnCollectionChanged(NotifyCollectionChangedAction.Move, fromVal, to, from);
        //--Replace-- OnCollectionChanged(NotifyCollectionChangedAction.Replace, toVal, fromVal, to);
    }

    [Serializable]
    private class SimpleMonitor : IDisposable
    {
        private int _busyCount;

        public bool Busy => _busyCount > 0;

        public void Enter()
        {
            _busyCount++;
        }

        public void Dispose()
        {
            _busyCount--;
        }
    }

    private const string CountString = "Count";

    private const string IndexerName = "Item[]";

    private readonly SimpleMonitor _monitor = new();

    //
    // Summary:
    //     Occurs when a property value changes.
    event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
    {
        add
        {
            PropertyChanged += value;
        }
        remove
        {
            PropertyChanged -= value;
        }
    }

    //
    // Summary:
    //     Occurs when an item is added, removed, changed, moved, or the entire list is
    //     refreshed.
    [field: NonSerialized]
    public event NotifyCollectionChangedEventHandler? CollectionChanged;

    //
    // Summary:
    //     Occurs when a property value changes.
    [field: NonSerialized]
    public event PropertyChangedEventHandler? PropertyChanged;

    //
    // Summary:
    //     Initializes a new instance of the System.Collections.ObjectModel.ObservableCollection`1
    //     class.
    public ObservableCollectionExtended()
    {
    }

    //
    // Summary:
    //     Initializes a new instance of the System.Collections.ObjectModel.ObservableCollection`1
    //     class that contains elements copied from the specified list.
    //
    // Parameters:
    //   list:
    //     The list from which the elements are copied.
    //
    // Exceptions:
    //   T:System.ArgumentNullException:
    //     The list parameter cannot be null.
    public ObservableCollectionExtended(List<T> list) : base((IList<T>)list)
    {
        CopyFrom(list);
    }

    //
    // Summary:
    //     Initializes a new instance of the System.Collections.ObjectModel.ObservableCollection`1
    //     class that contains elements copied from the specified collection.
    //
    // Parameters:
    //   collection:
    //     The collection from which the elements are copied.
    //
    // Exceptions:
    //   T:System.ArgumentNullException:
    //     The collection parameter cannot be null.
    public ObservableCollectionExtended(IEnumerable<T> collection)
    {
        if (collection == null)
        {
            throw new ArgumentNullException(nameof(collection));
        }

        CopyFrom(collection);
    }

    private void CopyFrom(IEnumerable<T> collection)
    {
        IList<T> items = base.Items;
        if (collection == null || items == null)
        {
            return;
        }

        foreach (T item in collection)
        {
            items.Add(item);
        }
    }

    //
    // Summary:
    //     Moves the item at the specified index to a new location in the collection.
    //
    // Parameters:
    //   oldIndex:
    //     The zero-based index specifying the location of the item to be moved.
    //
    //   newIndex:
    //     The zero-based index specifying the new location of the item.
    public void Move(int oldIndex, int newIndex)
    {
        MoveItem(oldIndex, newIndex);
    }

    //
    // Summary:
    //     Removes all items from the collection.
    protected override void ClearItems()
    {
        CheckReentrancy();
        base.ClearItems();
        OnPropertyChanged(CountString);
        OnPropertyChanged(IndexerName);
        OnCollectionReset();
    }

    //
    // Summary:
    //     Removes the item at the specified index of the collection.
    //
    // Parameters:
    //   index:
    //     The zero-based index of the element to remove.
    protected override void RemoveItem(int index)
    {
        CheckReentrancy();
        T val = base[index];
        base.RemoveItem(index);
        OnPropertyChanged(CountString);
        OnPropertyChanged(IndexerName);
        OnCollectionChanged(NotifyCollectionChangedAction.Remove, val, index);
    }

    //
    // Summary:
    //     Inserts an item into the collection at the specified index.
    //
    // Parameters:
    //   index:
    //     The zero-based index at which item should be inserted.
    //
    //   item:
    //     The object to insert.
    protected override void InsertItem(int index, T item)
    {
        CheckReentrancy();
        base.InsertItem(index, item);
        OnPropertyChanged(CountString);
        OnPropertyChanged(IndexerName);
        OnCollectionChanged(NotifyCollectionChangedAction.Add, item, index);
    }

    //
    // Summary:
    //     Replaces the element at the specified index.
    //
    // Parameters:
    //   index:
    //     The zero-based index of the element to replace.
    //
    //   item:
    //     The new value for the element at the specified index.
    protected override void SetItem(int index, T item)
    {
        CheckReentrancy();
        T val = base[index];
        base.SetItem(index, item);
        OnPropertyChanged(IndexerName);
        OnCollectionChanged(NotifyCollectionChangedAction.Replace, val, item, index);
    }

    //
    // Summary:
    //     Moves the item at the specified index to a new location in the collection.
    //
    // Parameters:
    //   oldIndex:
    //     The zero-based index specifying the location of the item to be moved.
    //
    //   newIndex:
    //     The zero-based index specifying the new location of the item.
    private void MoveItem(int oldIndex, int newIndex)
    {
        CheckReentrancy();
        T val = base[oldIndex];
        base.RemoveItem(oldIndex);
        base.InsertItem(newIndex, val);
        OnPropertyChanged(IndexerName);
        OnCollectionChanged(NotifyCollectionChangedAction.Move, val, newIndex, oldIndex);
    }

    //
    // Summary:
    //     Raises the System.Collections.ObjectModel.ObservableCollection`1.PropertyChanged
    //     event with the provided arguments.
    //
    // Parameters:
    //   e:
    //     Arguments of the event being raised.
    private void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        this.PropertyChanged?.Invoke(this, e);
    }

    //
    // Summary:
    //     Raises the System.Collections.ObjectModel.ObservableCollection`1.CollectionChanged
    //     event with the provided arguments.
    //
    // Parameters:
    //   e:
    //     Arguments of the event being raised.
    private void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
    {
        if (this.CollectionChanged != null)
        {
            using (BlockReentrancy())
            {
                this.CollectionChanged(this, e);
            }
        }
    }

    //
    // Summary:
    //     Disallows reentrant attempts to change this collection.
    //
    // Returns:
    //     An System.IDisposable object that can be used to dispose of the object.
    private IDisposable BlockReentrancy()
    {
        _monitor.Enter();
        return _monitor;
    }

    //
    // Summary:
    //     Checks for reentrant attempts to change this collection.
    //
    // Exceptions:
    //   T:System.InvalidOperationException:
    //     If there was a call to System.Collections.ObjectModel.ObservableCollection`1.BlockReentrancy
    //     of which the System.IDisposable return value has not yet been disposed of. Typically,
    //     this means when there are additional attempts to change this collection during
    //     a System.Collections.ObjectModel.ObservableCollection`1.CollectionChanged event.
    //     However, it depends on when derived classes choose to call System.Collections.ObjectModel.ObservableCollection`1.BlockReentrancy.
    private void CheckReentrancy()
    {
        if (_monitor.Busy && this.CollectionChanged != null && this.CollectionChanged.GetInvocationList().Length > 1)
        {
            throw new InvalidOperationException("ObservableCollectionReentrancyNotAllowed");
        }
    }

    private void OnPropertyChanged(string propertyName)
    {
        OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
    }

    private void OnCollectionChanged(NotifyCollectionChangedAction action, object item, int index)
    {
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, item, index));
    }

    private void OnCollectionChanged(NotifyCollectionChangedAction action, object item, int index, int oldIndex)
    {
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, item, index, oldIndex));
    }

    private void OnCollectionChanged(NotifyCollectionChangedAction action, object oldItem, object newItem, int index)
    {
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, newItem, oldItem, index));
    }

    private void OnCollectionReset()
    {
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }
}
