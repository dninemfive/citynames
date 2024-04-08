using d9.utl;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;

namespace citynames;
/// <summary>
/// Models a distribution across discrete variables.
/// </summary>
/// <typeparam name="K"><inheritdoc cref="IDistribution{K, V}" path="/typeparam[@name='K']"/></typeparam>
/// <typeparam name="V"><inheritdoc cref="IDistribution{K, V}" path="/typeparam[@name='V']"/></typeparam>
/// <param name="dict">A dictionary containing data for this distribution. If <see langword="null"/>,
///                    an empty distribution is initialized.</param>
public class DiscreteDistribution<K, V>(CountingDictionary<K, V>? dict = null)
    : IDistribution<K, V>, IEnumerable<KeyValuePair<K, V>>, IReadOnlyDictionary<K, V>
    where K : notnull
    where V : struct, IFloatingPoint<V>
{
    private readonly CountingDictionary<K, V> _dict = dict ?? new();
    /// <summary>
    /// The total weight of data points in this distribution.
    /// </summary>
    public V Weight { get; private set; } = dict?.Values.Sum() ?? V.Zero;

    public IEnumerable<K> Keys => _dict.Keys;

    public IEnumerable<V> Values => _dict.Values;

    public int Count => _dict.Count;

    /// <summary>
    /// Adds an item to this distribution.
    /// </summary>
    /// <param name="key">The key to which to add.</param>
    /// <param name="value">The value to add to the specified key. If <see langword="null"/>, defaults to 1.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the value added is negative.</exception>
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

    public bool ContainsKey(K key) => _dict.ContainsKey(key);

    public bool TryGetValue(K key, [MaybeNullWhen(false)] out V value) => _dict.TryGetValue(key, out value);
    /// <summary>
    /// Gets the probability that the specified <paramref name="key"/> would occur if this distribution were randomly sampled. 
    /// </summary>
    /// <param name="key">The key whose probability to get.</param>
    /// <returns>The probability that the key occurs. As a probability, it is normalized to the range [0..1].</returns>
    public V this[K key]
        => _dict.TryGetValue(key, out V value) ? value / Weight : V.Zero;
    /// <summary>
    /// Multiplies the values in this distribution by a constant factor.
    /// </summary>
    /// <param name="distribution">The distribution to multiply.</param>
    /// <param name="factor">The factor by which to multiply the variables.</param>
    /// <returns>A distribution corresponding to the input distribution with each variable multiplied by the specified factor.</returns>
    public static DiscreteDistribution<K, V> operator *(DiscreteDistribution<K, V> distribution, V factor)
        => new(distribution._dict * factor);
    /// <summary>
    /// Adds two distributions together.
    /// </summary>
    /// <param name="a">The first distribution to add.</param>
    /// <param name="b">The second distribution to add.</param>
    /// <returns>A new distribution where each variable has been added pairwise. If there is not a
    ///          corresponding variable in one or the other distribution, the variable will be included
    ///          unchanged, i.e. the set of variables is the union of the input variable sets.</returns>
    public static DiscreteDistribution<K, V> operator +(DiscreteDistribution<K, V> a, IReadOnlyDictionary<K, V> b)
        => new(a._dict + b);
}