using System;
using System.Collections.Generic;

public class BasisObservableList<T> : IList<T>
{
    private List<T> _list = new List<T>();

    public event Action OnListChanged;

    public T this[int index]
    {
        get => _list[index];
        set
        {
            _list[index] = value;
            OnListChanged?.Invoke();
        }
    }

    public int Count => _list.Count;

    public bool IsReadOnly => false;

    public void Add(T item)
    {
        _list.Add(item);
        OnListChanged?.Invoke();
    }

    public void Clear()
    {
        _list.Clear();
        OnListChanged?.Invoke();
    }

    public bool Contains(T item) => _list.Contains(item);

    public void CopyTo(T[] array, int arrayIndex) => _list.CopyTo(array, arrayIndex);

    public IEnumerator<T> GetEnumerator() => _list.GetEnumerator();

    public int IndexOf(T item) => _list.IndexOf(item);

    public void Insert(int index, T item)
    {
        _list.Insert(index, item);
        OnListChanged?.Invoke();
    }

    public bool Remove(T item)
    {
        bool result = _list.Remove(item);
        if (result)
        {
            OnListChanged?.Invoke();
        }
        return result;
    }

    public void RemoveAt(int index)
    {
        _list.RemoveAt(index);
        OnListChanged?.Invoke();
    }

    public int RemoveAll(Predicate<T> match)
    {
        int removedCount = _list.RemoveAll(match);
        if (removedCount > 0)
        {
            OnListChanged?.Invoke();
        }
        return removedCount;
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => _list.GetEnumerator();
}