using MathNet.Numerics;
using System.Numerics;
using System.Text.Json.Serialization;

namespace citynames;
public class VectorEncoding<T, U>
    where T : IEquatable<T>
    where U : struct, IFloatingPoint<U>
{
    [JsonInclude]
    public IReadOnlyList<T> Alphabet { get; private set; }
    [JsonIgnore]
    private readonly Dictionary<T, U[]> _encodingCache = new();
    [JsonConstructor]
    private VectorEncoding(IReadOnlyList<T>  alphabet)
    {
        Alphabet = alphabet;
    }
    public static VectorEncoding<T, U> OneHot(IEnumerable<T> items)
        => new([.. items.Distinct().Order()]);
    public U[] IndicatorVector(T item)
    {
        if (_encodingCache.TryGetValue(item, out U[]? cachedResult))
            return cachedResult;
        //Console.WriteLine(LogUtils.MethodArguments(arguments: [(nameof(item), item)]));
        if (!Alphabet.Contains(item))
            throw new ArgumentOutOfRangeException(nameof(item));
        U[] result = new U[DimensionCount];
        for (int i = 0; i < result.Length; i++)
            result[i] = item.Equals(Alphabet[i]) ? U.One : U.Zero;
        return result;
    }
    public U[] Encode(IReadOnlyDictionary<T, U> weights)
    {
        U totalWeight = weights.Where(x => Alphabet.Contains(x.Key)).Select(x => x.Value).Aggregate((x, y) => x + y);
        U[] result = new U[DimensionCount];
        for (int i = 0; i < result.Length; i++)
            result[i] = weights.TryGetValue(Alphabet[i], out U value) ? value / totalWeight : U.Zero;
        return result;
    }
    public T Decode(U[] encoded)
    {
        if (encoded.Length != Alphabet.Count)
            throw new ArgumentException($"Encoded vector has {encoded.Length} dimensions, but this encoding requires {DimensionCount}!", nameof(encoded));
        if (encoded.Any(x => x is not (0 or 1)) || encoded.Count(x => x == U.One) != 1)
            throw new ArgumentException($"OneHotEncoding.Decode can only decode vectors with exactly one 1-valued item; all others must be 0!", nameof(encoded));
        return Alphabet[encoded.IndexOfMaxValue()];
    }
    [JsonIgnore]
    public int DimensionCount => Alphabet.Count;
}