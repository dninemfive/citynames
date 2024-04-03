using MathNet.Numerics;
using System.Text.Json.Serialization;

namespace citynames;
[method: JsonConstructor]
public class BiomeCharacterRegression(OneHotEncoding<string> biomeEncoding, OneHotEncoding<char> characterEncoding, int offset)
{
    [JsonInclude]
    public OneHotEncoding<string> BiomeEncoding { get; private set; } = biomeEncoding;
    [JsonInclude]
    public OneHotEncoding<char> CharacterEncoding { get; private set; } = characterEncoding;
    [JsonInclude]
    public int Offset { get; private set; } = offset;
    [JsonIgnore]
    public int DimensionCount => BiomeEncoding.DimensionCount + CharacterEncoding.DimensionCount;
    public double[] Encode(string biome, char character)
        => BiomeEncoding.Encode(biome).AugmentWith(CharacterEncoding.Encode(character));
    public double[] Encode(IReadOnlyDictionary<string, double> biomeWeights, char character)
        => BiomeEncoding.Encode(biomeWeights).AugmentWith(CharacterEncoding.Encode(character));
    private readonly List<CharPair> _data = new();
    public void Add(CharPair pair)
    {
        if(pair.Offset == Offset)
        {
            _data.Add(pair);
            _model = null;
        }
    }
    public void AddMany(IEnumerable<CharPair> data)
    {
        foreach (CharPair pair in data)
            _data.Add(pair);
        _model = null;
    }
    public IEnumerable<(double[] xs, char result)> EncodedData
    {
        get
        {
            foreach (CharPair pair in _data)
                yield return (Encode(pair.Data.BiomeWeights, pair.Ancestor), pair.Result);
        }
    }
    private Dictionary<char, Func<double[], double>>? _model = null;
    [JsonIgnore]
    public Dictionary<char, Func<double[], double>> Model
    {
        get
        {
            if (_model is null)
            {
                _model = new();
                List <(double[] xs, char result)> encodedData = EncodedData.ToList();
                foreach (char character in encodedData.Select(x => x.result).Distinct().Order())
                {
                    List<(double[] xs, double weight)> relativeData = encodedData.Select(x => (x.xs, x.result == character ? 1.0 : 0)).ToList();
                    _model[character] = Fit.MultiDimFunc(relativeData.Select(x => x.xs).ToArray(), relativeData.Select(x => x.weight).ToArray());
                }
            }
            return _model;
        }
    }
    public double WeightFor(char character, string biome, char ancestor)
        => Model[character](Encode(biome, ancestor));
    public IReadOnlyDictionary<char, double> WeightsFor(string biome, char ancestor)
    {
        Console.WriteLine(LogUtils.MethodArguments(arguments: [(nameof(biome), biome), (nameof(ancestor), ancestor)]));
        Dictionary<char, double> result = new();
        foreach ((char c, Func<double[], double> weight) in Model)
        {
            Console.WriteLine($"\t{c}");
            result[c] = weight(Encode(biome, ancestor)) / Offset;
        }
        return result;
    }
}