using d9.utl;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace citynames;
[Generator("regression", "regression_{contextLength}.json")]
public class RegressionStringGenerator : IBuildLoadableStringGenerator<CityInfo, RegressionStringGenerator>
{
    [JsonInclude]
    public BiomeCharacterRegressionSet Model { get; private set; }
    [JsonInclude]
    public int MaxOffset { get; private set; }
    private RegressionStringGenerator(BiomeCharacterRegressionSet model, int maxOffset)
    {
        Model = model;
        MaxOffset = maxOffset;
    }
    public static RegressionStringGenerator Build(IEnumerable<(string item, CityInfo metadata)> input, int contextLength)
    {
        Console.WriteLine(LogUtils.MethodArguments(arguments: [(nameof(input), input), (nameof(contextLength), contextLength)]));
        OneHotEncoding<string> biomeEncoding = new(input.Select(x => x.metadata.Biome));
        OneHotEncoding<char> characterEncoding = new(input.SelectMany(x => x.item.SandwichWith(Characters.START, Characters.STOP)));
        BiomeCharacterRegressionSet model = new(biomeEncoding, characterEncoding, contextLength);        
        return new(model, contextLength);
    }
    public static RegressionStringGenerator Load(string path)
        => JsonSerializer.Deserialize<RegressionStringGenerator>(File.ReadAllText(path))!;
    private char RandomChar(CityInfo input, string context)
    {
        Console.WriteLine(LogUtils.MethodArguments(arguments: [(nameof(input), input), (nameof(context), context)]));
        CountingDictionary<char, double> dict = new();
        for(int offset = 1; offset <= Model.MaxOffset; offset++)
        {
            int i = context.Length - offset;
            if (i < 0)
                break;
            dict += Model.WeightsFor(input.Biome, context[i]);
        }
        return dict.WeightedRandomElement();
    }
    public string RandomString(CityInfo input, int minLength, int maxLength)
    {
        string result = $"{Characters.START}";
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
        File.WriteAllText(path, JsonSerializer.Serialize(this));
        return Task.CompletedTask;
    }
}