namespace citynames;
public class MulticlassFeatures(IReadOnlyDictionary<string, float> biomeWeights, string[] ancestors, string result)
{

    public IReadOnlyDictionary<string, float> BiomeWeights = biomeWeights;
    public string[] Ancestors = ancestors;
    public string Result = result;
    public MulticlassFeatures(string biome, string[] ancestors, string result)
        : this(biome.ToOneHotVector<float>(), ancestors, result) { }
    public static IEnumerable<MulticlassFeatures> From(string cityName, CityInfo cityInfo, int contextLength = Defaults.CONTEXT_LENGTH)
    {
        Console.WriteLine(LogUtils.Method(args: [(nameof(cityName), cityName), (nameof(cityInfo), cityInfo), (nameof(contextLength), contextLength)]));
        for(int i = 0; i < cityName.Length; i++)
        {
            string[] ancestors = new string[contextLength];
            for(int j = 0; j < contextLength; j++)
            {
                int ancestorIndex = i - (contextLength - j);
                ancestors[j] = ancestorIndex >= 0 ? $"{cityName[ancestorIndex]}" : "";
            }
            yield return new(cityInfo.Biome, ancestors, $"{cityName[i]}");
        }
    }
    public static IEnumerable<MulticlassFeatures> From(IEnumerable<(string cityName, CityInfo cityInfo)> corpus,
                                                       int contextLength = Defaults.CONTEXT_LENGTH)
        => corpus.SelectMany(x => From(x.cityName, x.cityInfo, contextLength));
    public static MulticlassFeatures Query(string biome)
        => Query(biome.ToOneHotVector<float>(), []);
    public static MulticlassFeatures Query(IReadOnlyDictionary<string, float> biomeWeights, string[] ancestors)
        => new(biomeWeights, ancestors, "");
}
public class EncodedMulticlassFeatures(MulticlassFeatures mf, VectorEncoding<string, float> biomeEncoding, VectorEncoding<string, float> characterEncoding)
{
    public float[] BiomeWeights = biomeEncoding.Encode(mf.BiomeWeights);
    public float[] JoinedAncestorWeights = mf.Ancestors.SelectMany(characterEncoding.Encode).ToArray();
    public string Result = mf.Result;
}