using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;

namespace Leditor.Data.Generic;

public class ObservableDictionary<TKey, TValue> : IDictionary<TKey, TValue>, INotifyCollectionChanged
    where TKey : IEquatable<TKey>
{
    private readonly IDictionary<TKey, TValue> _dictionary;

    public ObservableDictionary()
    {
        _dictionary = new Dictionary<TKey, TValue>();
    }

    public ObservableDictionary(IEnumerable<KeyValuePair<TKey, TValue>> pairs)
    {
        _dictionary = new Dictionary<TKey, TValue>(pairs);
    }

    //

    public TValue this[TKey key]
    { 
        get => _dictionary[key]; 
        set
        {
            _dictionary[key] = value;
            CollectionChanged?.Invoke(this, new(NotifyCollectionChangedAction.Replace, value));
        } 
    }

    public ICollection<TKey> Keys => _dictionary.Keys;

    public ICollection<TValue> Values => _dictionary.Values;

    public int Count => _dictionary.Count;

    public bool IsReadOnly => _dictionary.IsReadOnly;

    public event NotifyCollectionChangedEventHandler? CollectionChanged;

    public void Add(TKey key, TValue value)
    {
        _dictionary.Add(key, value);

        CollectionChanged?.Invoke(this, new(NotifyCollectionChangedAction.Add, value));
    }

    public void Add(KeyValuePair<TKey, TValue> item)
    {
        _dictionary.Add(item);

        CollectionChanged?.Invoke(this, new(NotifyCollectionChangedAction.Add, item.Value));
    }

    public void Clear()
    {
        _dictionary.Clear();

        CollectionChanged?.Invoke(this, new(NotifyCollectionChangedAction.Reset));
    }

    public bool Contains(KeyValuePair<TKey, TValue> item) => _dictionary.Contains(item);

    public bool ContainsKey(TKey key) => _dictionary.ContainsKey(key);

    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
        _dictionary.CopyTo(array, arrayIndex);
    }

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => _dictionary.GetEnumerator();

    public bool Remove(TKey key)
    {
        var itemRetrieved = _dictionary.TryGetValue(key, out var item);

        if (!itemRetrieved) return false;

        var result = _dictionary.Remove(key);

        if (result) {
            CollectionChanged?.Invoke(this, new(NotifyCollectionChangedAction.Remove, item));
        }

        return result;
    }

    public bool Remove(KeyValuePair<TKey, TValue> item)
    {
        var result = _dictionary.Remove(item);
    
        if (result) {
            CollectionChanged?.Invoke(this, new(NotifyCollectionChangedAction.Remove, item.Value));
        }

        return result;
    }

    public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value) => _dictionary.TryGetValue(key, out value);

    IEnumerator IEnumerable.GetEnumerator()
    {
        return _dictionary.GetEnumerator();
    }
}
