using d9.utl;
using MathNet.Numerics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace citynames.Utils;
public class OneHotEncoding<T>(IEnumerable<T> items)
    where T : IEquatable<T>
{
    private readonly List<T> _alphabet = [.. items.Distinct().Order()];
    public IEnumerable<T> Alphabet => _alphabet;
    public double[] Encode(T item)
    {
        if (!_alphabet.Contains(item))
            throw new ArgumentOutOfRangeException(nameof(item));
        double[] result = new double[DimensionCount];
        for (int i = 0; i < result.Length; i++)
            result[i] = item.Equals(_alphabet[i]) ? 1 : 0;
        return result;
    }
    public double[] Encode(IReadOnlyDictionary<T, double> weights)
    {
        double totalWeight = weights.Where(x => Alphabet.Contains(x.Key)).Sum(x => x.Value);
        double[] result = new double[DimensionCount];
        for (int i = 0; i < result.Length; i++)
            result[i] = weights.TryGetValue(_alphabet[i], out double value) ? value / totalWeight : 0;
        return result;
    }
    public T Decode(double[] encoded)
    {
        if (encoded.Length != _alphabet.Count)
            throw new ArgumentException($"Encoded vector has {encoded.Length} dimensions, but this encoding requires {DimensionCount}!", nameof(encoded));
        if (encoded.Any(x => x is not (0 or 1)) || encoded.Count(x => x == 1) != 1)
            throw new ArgumentException($"OneHotEncoding.Decode can only decode vectors with exactly one 1-valued item; all others must be 0!", nameof(encoded));
        return _alphabet[encoded.IndexOfMaxValue()];
    }
    public int DimensionCount => _alphabet.Count;
}
public class BiomeCharacterRegression(OneHotEncoding<string> biomeEncoding, OneHotEncoding<char> characterEncoding, int offset)
{
    public OneHotEncoding<string> BiomeEncoding { get; private set; } = biomeEncoding;
    public OneHotEncoding<char> CharacterEncoding { get; private set; } = characterEncoding;
    public int Offset { get; private set; } = offset;
    public int DimensionCount => BiomeEncoding.DimensionCount + CharacterEncoding.DimensionCount;
    public double[] Encode(string biome, char character)
        => BiomeEncoding.Encode(biome).AugmentWith(CharacterEncoding.Encode(character));
    private readonly List<(string biome, char ancestor, char result)> _data = new();
    public void Add(string biome, char ancestor, char result)
    {
        _data.Add((biome, ancestor, result));
        _model = null;
    }
    public IEnumerable<(double[] xs, char result)> EncodedData
    {
        get
        {
            foreach ((string biome, char ancestor, char result) in _data)
                yield return (Encode(biome, ancestor), result);
        }
    }
    private Dictionary<char, Func<double[], double>>? _model = null;
    public Dictionary<char, Func<double[], double>> Model
    {
        get
        {
            if(_model is null)
            {
                _model = new();
                List <(double[] xs, char result)> encodedData = EncodedData.ToList();
                foreach(char character in encodedData.Select(x => x.result).Distinct().Order())
                {
                    List<(double[] xs, double weight)> relativeData = encodedData.Select(x => (x.xs, x.result == character ? 1.0 : 0)).ToList();
                    _model[character] = Fit.MultiDimFunc(relativeData.Select(x => x.xs).ToArray(), relativeData.Select(x => x.weight).ToArray());
                }
            }
            return _model;
        }
    }
    public double WeightFor(char character, string biome, char ancestor)
        => Model[character](Encode(biome, ancestor));
    public IReadOnlyDictionary<char, double> WeightsFor(string biome, char ancestor)
    {
        Dictionary<char, double> result = new();
        foreach((char c, Func<double[], double> weight) in Model)
            result[c] = weight(Encode(biome, ancestor)) / Offset;
        return result;
    }
}
public class BiomeCharacterRegressionSet
{
    public OneHotEncoding<string> BiomeEncoding { get; private set; }
    public OneHotEncoding<char> AncestorEncoding { get; private set; }
    public int MaxOffset { get; private set; }
    private readonly Dictionary<int, BiomeCharacterRegression> _regressions = new();
    public BiomeCharacterRegressionSet(OneHotEncoding<string> biomeEncoding, OneHotEncoding<char> ancestorEncoding, int maxOffset)
    {
        BiomeEncoding = biomeEncoding;
        AncestorEncoding = ancestorEncoding;
        MaxOffset = maxOffset;
        for (int i = 1; i < maxOffset; i++)
            _regressions[i] = new(biomeEncoding, ancestorEncoding, i);
    }
    public IReadOnlyDictionary<char, double> WeightsFor(string biome, char ancestor)
    {
        DefaultDict<char, double> result = new(default(double));
        foreach ((int offset, BiomeCharacterRegression regression) in _regressions)
        {
            foreach ((char character, double weight) in regression.WeightsFor(biome, ancestor))
                result[character] += weight;
        }
        return (IReadOnlyDictionary<char, double>)result;
    }
}
public class DefaultDict<K, V>(Func<K, V> defaultFn) : IDictionary<K, V>
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