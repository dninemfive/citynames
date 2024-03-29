using System.Numerics;

namespace citynames;
/// <summary>
/// Abstraction for mapping keys to floating-point values.
/// </summary>
/// <typeparam name="K">The type of the input variable for this distribution.</typeparam>
/// <typeparam name="V">The type of the output variable for this distribution.</typeparam>
public interface IDistribution<K, V>
    where K : notnull
    where V : IFloatingPoint<V>
{
    public V this[K key] { get; }
}