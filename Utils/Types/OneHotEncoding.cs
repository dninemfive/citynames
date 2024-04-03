using MathNet.Numerics;
using System.Text.Json.Serialization;

namespace citynames;
public class OneHotEncoding<T>
    where T : IEquatable<T>
{
    [JsonInclude]
    public IReadOnlyList<T> Alphabet { get; private set; }
    [JsonConstructor]
    private OneHotEncoding(IReadOnlyList<T>  alphabet)
    {
        Alphabet = alphabet;
    }
    public static OneHotEncoding<T> From(IEnumerable<T> items)
        => new([.. items.Distinct().Order()]);
    public double[] Encode(T item)
    {
        //Console.WriteLine(LogUtils.MethodArguments(arguments: [(nameof(item), item)]));
        if (!Alphabet.Contains(item))
            throw new ArgumentOutOfRangeException(nameof(item));
        double[] result = new double[DimensionCount];
        for (int i = 0; i < result.Length; i++)
            result[i] = item.Equals(Alphabet[i]) ? 1 : 0;
        return result;
    }
    public double[] Encode(IReadOnlyDictionary<T, double> weights)
    {
        double totalWeight = weights.Where(x => Alphabet.Contains(x.Key)).Sum(x => x.Value);
        double[] result = new double[DimensionCount];
        for (int i = 0; i < result.Length; i++)
            result[i] = weights.TryGetValue(Alphabet[i], out double value) ? value / totalWeight : 0;
        return result;
    }
    public T Decode(double[] encoded)
    {
        if (encoded.Length != Alphabet.Count)
            throw new ArgumentException($"Encoded vector has {encoded.Length} dimensions, but this encoding requires {DimensionCount}!", nameof(encoded));
        if (encoded.Any(x => x is not (0 or 1)) || encoded.Count(x => x == 1) != 1)
            throw new ArgumentException($"OneHotEncoding.Decode can only decode vectors with exactly one 1-valued item; all others must be 0!", nameof(encoded));
        return Alphabet[encoded.IndexOfMaxValue()];
    }
    [JsonIgnore]
    public int DimensionCount => Alphabet.Count;
}