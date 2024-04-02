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
public class BiomeCharacterRegression(OneHotEncoding<string> biomeEncoding, OneHotEncoding<char> ancestorEncoding, int offset)
{
    public OneHotEncoding<string> BiomeEncoding { get; private set; } = biomeEncoding;
    public OneHotEncoding<char> AncestorEncoding { get; private set; } = ancestorEncoding;
    public int Offset { get; private set; } = offset;
    public int DimensionCount => BiomeEncoding.DimensionCount + AncestorEncoding.DimensionCount;
    public double[] Encode(string biome, char character)
        => BiomeEncoding.Encode(biome).AugmentWith(AncestorEncoding.Encode(character));
}
public class BiomeCharacterRegressionSet(OneHotEncoding<string> biomeEncoding, OneHotEncoding<char> ancestorEncoding, int maxOffset)
{
    public OneHotEncoding<string> BiomeEncoding { get; private set; } = biomeEncoding;
    public OneHotEncoding<char> AncestorEncoding { get; private set; } = ancestorEncoding;
    public int MaxOffset { get; private set; } = maxOffset;
    private Dictionary<int, BiomeCharacterRegression> _regressions = [.. 1.To(maxOffset).Select(x => new KeyValuePair<int, BiomeCharacterRegression>(x, new BiomeCharacterRegression(biomeEncoding, ancestorEncoding, x)))];
}