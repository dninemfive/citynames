using System.Numerics;

namespace citynames;
public class MulticlassFeatures(float[] biomeWeights, float[] ancestors, string result)
{

    public float[] BiomeWeights = biomeWeights;
    public float[] Ancestors = ancestors;
    public string Result = result;
    public MulticlassFeatures(DictionaryEncoding<string, float> biomes, ArrayEncoding<string, float> ancestors, string result)
        : this(biomes.Encode(), ancestors.Encode(), result) { }
    public MulticlassFeatures((string value, VectorEncoding<string, float> encoding) biome, ArrayEncoding<string, float> ancestors, string result)
        : this(biome.encoding.Encode(biome.value), ancestors.Encode(), result) { }
    public static IEnumerable<MulticlassFeatures> From(string cityName, CityInfo cityInfo, VectorEncoding<string, float> characterEncoding, VectorEncoding<string, float> biomeEncoding, int contextLength = Defaults.CONTEXT_LENGTH)
    {
        Console.WriteLine(LogUtils.Method(args: [(nameof(cityName), cityName), (nameof(cityInfo), cityInfo), (nameof(biomeEncoding), biomeEncoding)]));
        for(int i = 0; i < cityName.Length; i++)
        {
            string[] ancestors = new string[contextLength];
            for(int j = 0; j < contextLength; j++)
            {
                int ancestorIndex = i - (contextLength - j);
                ancestors[j] = ancestorIndex >= 0 ? $"{cityName[ancestorIndex]}" : "";
            }
            yield return new((cityInfo.Biome, biomeEncoding), new(ancestors, characterEncoding), $"{cityName[i]}");
        }
    }
    public static IEnumerable<MulticlassFeatures> From(IEnumerable<(string cityName, CityInfo cityInfo)> corpus,
                                                       VectorEncoding<string, float> characterEncoding,
                                                       VectorEncoding<string, float> biomeEncoding,
                                                       int contextLength = Defaults.CONTEXT_LENGTH)
        => corpus.SelectMany(x => From(x.cityName, x.cityInfo, characterEncoding, biomeEncoding, contextLength));
    public static MulticlassFeatures Query(float[] biomeWeights)
        => Query(biomeWeights, []);
    public static MulticlassFeatures Query(float[] biomeWeights, ArrayEncoding<string, float> ancestors)
        => new(biomeWeights, ancestors.Encode(), "");
}
public class DictionaryEncoding<T, U>(IReadOnlyDictionary<T, U> data, VectorEncoding<T, U> encoding)
{
    public IReadOnlyDictionary<T, U> Data = data;
    public VectorEncoding<T, U> Encoding = encoding;
    public U[] Encode()
        => Encoding.Encode(Data);
}
public class ArrayEncoding<T, U>(T[] data, VectorEncoding<T, U> encoding)
    where T : IEquatable<T>
    where U : struct, IFloatingPoint<U>
{
    public readonly T[] Data = data;
    public readonly VectorEncoding<T, U> Encoding = encoding;
    public U[] Encode()
    {
        U[] result = new U[Data.Length * Encoding.DimensionCount];
        for(int i = 0; i < Data.Length; i++)
        {
            U[] encodedItem = Encoding.Encode(Data[i]);
            for(int j = 0; j < encodedItem.Length; j++)
                result[i * Encoding.DimensionCount + j] = encodedItem[j];
        }
        return result;
    }
}