
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