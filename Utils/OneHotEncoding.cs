using MathNet.Numerics;
using System;
using System.Collections.Generic;
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
        _coefficients = null;
    }
    public IEnumerable<(double[] xs, char result)> EncodedData
    {
        get
        {
            foreach ((string biome, char ancestor, char result) in _data)
                yield return (Encode(biome, ancestor), result);
        }
    }
    private Dictionary<char, double[]>? _coefficients = null;
    public Dictionary<char, double[]> Coefficiencts
    {
        get
        {
            if(_coefficients is null)
            {
                _coefficients = new();
                Dictionary<char, List<double[]>> encodedDataByCharacter = EncodedData.GroupBy(x => x.result)
                                                                                     .Select(x => new KeyValuePair<char, List<double[]>>(x.Key, x.Select(x => x.xs)
                                                                                                                                                 .ToList()))
                                                                                     .ToDictionary();
                foreach((char character, List <double[]> data) in encodedDataByCharacter)
                {
                    _coefficients[character] = 
                }
            }
            return _coefficients;
        }
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
    public void Add(CharPair pair, string biome)
    {

    }
}