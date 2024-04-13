using d9.utl;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace citynames;
// [Generator("regression", "regression_{contextLength}.json")]
public class RegressionStringGenerator : IBuildLoadableStringGenerator<CityInfo, RegressionStringGenerator>
{
    [JsonInclude]
    public AncestorCharacterRegression Model { get; private set; }
    [JsonInclude]
    public int MaxOffset { get; private set; }
    [JsonConstructor]
    private RegressionStringGenerator(AncestorCharacterRegression model, int maxOffset)
    {
        Model = model;
        MaxOffset = maxOffset;
    }
    public static RegressionStringGenerator Build(IEnumerable<(string item, CityInfo metadata)> input, int contextLength)
    {
        Console.WriteLine(LogUtils.Method(args: [(nameof(input), input), (nameof(contextLength), contextLength)]));
        VectorEncoding<string, double> biomeEncoding = VectorEncoding<string, double>.From(input.Select(x => x.metadata.Biome));
        List<(string item, CityInfo)> processedInput = input.Select(x => (x.item.SandwichWith(Characters.START, Characters.STOP),
                                                                              x.metadata)).ToList();
        VectorEncoding<char, double> characterEncoding = VectorEncoding<char, double>.From(processedInput.SelectMany(x => x.item));
        AncestorCharacterRegression model = AncestorCharacterRegression.FromData(processedInput, contextLength);
        return new(model, contextLength);
    }
    public static RegressionStringGenerator Load(string path)
        => JsonSerializer.Deserialize<RegressionStringGenerator>(File.ReadAllText(path))!;
    private char RandomChar(CityInfo input, string context)
    {
        Console.WriteLine(LogUtils.Method(args: [(nameof(input), input), (nameof(context), context)]));
        return Model.WeightsFor(new(input.Biome), context).WeightedRandomElement();
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
        return result.Replace($"{Characters.START}","");
    }
    private static readonly JsonSerializerOptions _options = new()
    {
        WriteIndented = true,
        NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals
    };
    public Task SaveAsync(string path)
    {
        File.WriteAllText(path, JsonSerializer.Serialize(this, _options));
        return Task.CompletedTask;
    }
}