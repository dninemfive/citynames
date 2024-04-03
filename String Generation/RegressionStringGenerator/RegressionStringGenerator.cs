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
    [JsonConstructor]
    private RegressionStringGenerator(BiomeCharacterRegressionSet model, int maxOffset)
    {
        Model = model;
        MaxOffset = maxOffset;
    }
    public static RegressionStringGenerator Build(IEnumerable<(string item, CityInfo metadata)> input, int contextLength)
    {
        Console.WriteLine(LogUtils.MethodArguments(arguments: [(nameof(input), input), (nameof(contextLength), contextLength)]));
        OneHotEncoding<string> biomeEncoding = OneHotEncoding<string>.From(input.Select(x => x.metadata.Biome));
        List<(string item, string biome)> processedInput = input.Select(x => (x.item.SandwichWith(Characters.START, Characters.STOP),
                                                                              x.metadata.Biome)).ToList();
        OneHotEncoding<char> characterEncoding = OneHotEncoding<char>.From(processedInput.SelectMany(x => x.item));
        BiomeCharacterRegressionSet model = new(biomeEncoding, characterEncoding, contextLength);
        model.AddMany(processedInput);
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
            Console.WriteLine($"\t{offset} ({i})");
            if (i < 0)
                break;
            dict += Model.WeightsFor(input.Biome, context[i]);
        }
        if(dict.Any())
        {
            Console.WriteLine($"{dict.Select(x => $"{x.Key}: {x.Value}").ListNotation()}");
            return dict.WeightedRandomElement();
        }
        Console.WriteLine($"No weights found.");
        return Characters.STOP;
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