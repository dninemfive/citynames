using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace citynames;
public class DefaultDict<K, V>(Func<K, V> defaultFn) : IDictionary<K, V>, IReadOnlyDictionary<K, V>
    where K : notnull
{
    public readonly Func<K, V> DefaultFor = defaultFn;
    public DefaultDict(V defaultValue) : this(_ => defaultValue) { }
    private readonly Dictionary<K, V> _dict = new();
    public V this[K key]
    {
        get => _dict.TryGetValue(key, out V? result) ? result : DefaultFor(key);
        set => _dict[key] = value;
    }
    public ICollection<K> Keys => _dict.Keys;
    public ICollection<V> Values => _dict.Values;
    public int Count => _dict.Count;
    public bool IsReadOnly => false;

    IEnumerable<K> IReadOnlyDictionary<K, V>.Keys => ((IReadOnlyDictionary<K, V>)_dict).Keys;

    IEnumerable<V> IReadOnlyDictionary<K, V>.Values => ((IReadOnlyDictionary<K, V>)_dict).Values;

    public void Add(K key, V value) => _dict.Add(key, value);
    public void Add(KeyValuePair<K, V> item) => _dict.Add(item.Key, item.Value);
    public void Add((K key, V value) tuple) => _dict.Add(tuple.key, tuple.value);
    public void Clear() => _dict.Clear();
    public bool Contains(KeyValuePair<K, V> item) => _dict.Contains(item);
    public bool ContainsKey(K key) => _dict.ContainsKey(key);
    public void CopyTo(KeyValuePair<K, V>[] array, int arrayIndex)
    {
        // ...i think this is what this is supposed to do?
        for (int i = arrayIndex; i < array.Length; i++)
            _dict.Add(array[i].Key, array[i].Value);
    }
    public IEnumerator<KeyValuePair<K, V>> GetEnumerator() => _dict.GetEnumerator();
    public bool Remove(K key) => _dict.Remove(key);
    public bool Remove(KeyValuePair<K, V> item) => _dict.Remove(item.Key);
    public bool TryGetValue(K key, [MaybeNullWhen(false)] out V value) => _dict.TryGetValue(key, out value);
    IEnumerator IEnumerable.GetEnumerator() => _dict.GetEnumerator();
}