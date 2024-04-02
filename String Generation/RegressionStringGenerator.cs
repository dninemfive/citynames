
using citynames.Utils;
using d9.utl;

namespace citynames;
public class RegressionStringGenerator : IBuildLoadableStringGenerator<CityInfo, RegressionStringGenerator>
{
    public BiomeCharacterRegressionSet Model { get; private set; }
    private RegressionStringGenerator(BiomeCharacterRegressionSet model)
    {
        Model = model;
    }
    public static RegressionStringGenerator Build(IEnumerable<(string item, CityInfo metadata)> input, int contextLength)
    {
        OneHotEncoding<string> biomeEncoding = new(input.Select(x => x.metadata.Biome));
        OneHotEncoding<char> characterEncoding = new(input.SelectMany(x => x.item));
        BiomeCharacterRegressionSet model = new(biomeEncoding, characterEncoding, contextLength);
        return new(model);
    }
    public static RegressionStringGenerator Load(string path)
    {
        throw new NotImplementedException();
    }
    private char RandomChar(CityInfo input, string context)
    {
        DiscreteDistribution<char, double> distribution = new();
        for(int offset = 1; offset <= Model.MaxOffset; offset++)
        {
            int i = context.Length - offset;
            if (i <= 0)
                break;
            distribution += Model.WeightsFor(input.Biome, context[i]);
        }
        return distribution.WeightedRandomElement();
    }
    public string RandomString(CityInfo input, int minLength, int maxLength)
    {
        string result = "";
        char cur = RandomChar(input, result);
        while(result.Length < maxLength && cur != Characters.STOP)
        {
            result += cur;
            cur = RandomChar(input, result);
        }
        return result;
    }
    public Task SaveAsync(string path)
    {
        throw new NotImplementedException();
    }
}
public class CharPair(char ancestor, char successor, int offset, QueryInfo data)
{
    public readonly char Ancestor = ancestor, Successor = successor;
    // e.g. 1 = {ancestor}{successor}, 2 = {ancestor}.{successor}, 3 = {ancestor}..{successor}
    public readonly int Offset = offset;
    public readonly QueryInfo Data = data;
    public static IEnumerable<CharPair> From(string cityName, string biome, int maxOffset = 4)
    {
        for(int i = 0; i < cityName.Length; i++)
        {
            char ancestor = cityName[i];
            for(int j = i + 1; i < cityName.Length; i++)
            {
                char successor = cityName[j];
                int offset = j - i;
                if (offset > maxOffset)
                    break;
                yield return new(ancestor, successor, offset, new(biome));
            }
        }
    }
    public static IEnumerable<CharPair> From((string cityName, string biome) tuple, int maxOffset = 4)
        => From(tuple.cityName, tuple.biome, maxOffset);
    public static IEnumerable<CharPair> From(IEnumerable<(string cityName, string biome)> tuples, int maxOffset = 4)
        => tuples.SelectMany(x => From(x, maxOffset)).ToList();
}
public class QueryInfo(IReadOnlyDictionary<string, float> biomeWeights)
{
    public IReadOnlyDictionary<string, float> BiomeWeights = biomeWeights;
    public QueryInfo(string biome) : this(biome.ToWeightVector()) { }
}