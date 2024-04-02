using MathNet.Numerics;

namespace citynames;
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