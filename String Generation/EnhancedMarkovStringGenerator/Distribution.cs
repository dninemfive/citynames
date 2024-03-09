using d9.utl;
using System.Collections;
using System.Numerics;

namespace citynames;
public interface IDistribution<K, V>
    where K : notnull
    where V : IFloatingPoint<V>
{
    public V this[K key] { get; }
}
public class DiscreteDistribution<K, V>(CountingDictionary<K, V>? dict = null)
    : IDistribution<K, V>, IEnumerable<KeyValuePair<K, V>>
    where K : notnull
    where V : struct, IFloatingPoint<V>
{
    private readonly CountingDictionary<K, V> _dict = dict ?? new();
    public V Weight { get; private set; } = dict?.Values.Sum() ?? V.Zero;
    public void Add(K key, V? value = null)
    {
        if (value is not null && value <= V.Zero)
            throw new ArgumentOutOfRangeException(nameof(value));
        Weight += value ?? V.One;
        _dict[key] += value ?? V.One;
    }
    public IEnumerator<KeyValuePair<K, V>> GetEnumerator()
        => ((IEnumerable<KeyValuePair<K, V>>)_dict).GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable)_dict).GetEnumerator();
    public V this[K key]
        => _dict.TryGetValue(key, out V value) ? value / Weight : V.Zero;
    public static DiscreteDistribution<K, V> operator *(DiscreteDistribution<K, V> distribution, V factor)
        => new(distribution._dict * factor);
    public static DiscreteDistribution<K, V> operator +(DiscreteDistribution<K, V> a, DiscreteDistribution<K, V> b)
        => new(a._dict + b._dict);
}