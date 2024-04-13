using MathNet.Numerics;
using System.Numerics;
using System.Text;
using System.Text.Json.Serialization;

namespace citynames;
public class VectorEncoding<T, U>
    where T : IEquatable<T>
    where U : struct, IFloatingPoint<U>
{
    [JsonInclude]
    public T? Default { get; private set; }
    [JsonInclude]
    public IReadOnlyList<T> Alphabet { get; private set; }
    [JsonIgnore]
    private readonly Dictionary<T, U[]> _encodingCache = new();
    [JsonConstructor]
    private VectorEncoding(IReadOnlyList<T> alphabet, T? @default = default)
    {
        Alphabet = alphabet;
        Default = @default;
    }
    public static VectorEncoding<T, U> From(IEnumerable<T> items, T? @default = default)
        => new([.. items.Distinct().Where(x => !x.Equals(@default)).Order()], @default);
    public U[] Encode(T item, bool throwIfNotInAlphabet = false)
    {
        if (_encodingCache.TryGetValue(item, out U[]? cachedResult))
            return cachedResult;
        //Console.WriteLine(LogUtils.MethodArguments(arguments: [(nameof(item), item)]));
        if (throwIfNotInAlphabet && !Alphabet.Contains(item))
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
    public T DecodeOneHot(U[] encoded)
    {
        if (encoded.Length != Alphabet.Count)
            throw new ArgumentException($"Encoded vector has {encoded.Length} dimensions, but this encoding requires {DimensionCount}!", nameof(encoded));
        if(encoded.All(U.IsZero))
            return Default ?? throw DefaultIsNull(nameof(encoded));
        if (encoded.Any(x => x != U.Zero || x != U.One))
            throw NotOneHot(nameof(encoded));
        if (encoded.Count(x => x == U.One) != 1)
            throw MoreThanOneHotValue(nameof(encoded));
        return Alphabet[encoded.IndexOfMaxValue()];
    }
    #region exceptions
    private ArgumentException DimensionMismatch(string name, int length)
        => new($"Encoded vector has {length} dimensions, but this encoding requires {DimensionCount}!", name);
    private ArgumentException DefaultIsNull(string name)
        => new($"Attempted to decode an all-zero array, but {this} does not have a default value!", name);
    private static ArgumentException NotOneHot(string name)
        => new($"Attempted to decode an array as one-hot, but it had values which are not zero or one!", name);
    private static ArgumentException MoreThanOneHotValue(string name)
        => new($"Attempted to decode an array as one-hot, but it had multiple hot values!", name);
    #endregion exceptions
    public IEnumerable<T> Decode(U[] encoded, U? threshold = null)
    {
        if (encoded.Length != Alphabet.Count)
            throw DimensionMismatch(nameof(encoded), encoded.Length);
        threshold ??= encoded.Max();
        for(int i = 0; i < encoded.Length; i++)
        {
            if (encoded[i] >= threshold)
                yield return Alphabet[i];
        }
    }
    [JsonIgnore]
    public int DimensionCount => Alphabet.Count;
    public T this[int index] => Alphabet[index];
}
public static class VectorEncodingExtensions
{
    public static VectorEncoding<T, U> OneHotEncodingFrom<T, U>(this IEnumerable<T> items)
        where T : IEquatable<T>
        where U : struct, IFloatingPoint<U>
        => VectorEncoding<T, U>.From([.. items.Distinct().Order()]);
}