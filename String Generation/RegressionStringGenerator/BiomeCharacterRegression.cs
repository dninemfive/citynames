using MathNet.Numerics;

namespace citynames;
public class BiomeCharacterRegression(OneHotEncoding<string> biomeEncoding, OneHotEncoding<char> characterEncoding, int offset)
{
    public OneHotEncoding<string> BiomeEncoding { get; private set; } = biomeEncoding;
    public OneHotEncoding<char> CharacterEncoding { get; private set; } = characterEncoding;
    public int Offset { get; private set; } = offset;
    public int DimensionCount => BiomeEncoding.DimensionCount + CharacterEncoding.DimensionCount;
    public double[] Encode(string biome, char character)
        => BiomeEncoding.Encode(biome).AugmentWith(CharacterEncoding.Encode(character));
    private readonly List<(string biome, char ancestor, char result)> _data = new();
    public void Add(string biome, char ancestor, char result)
    {
        _data.Add((biome, ancestor, result));
        _model = null;
    }
    public IEnumerable<(double[] xs, char result)> EncodedData
    {
        get
        {
            foreach ((string biome, char ancestor, char result) in _data)
                yield return (Encode(biome, ancestor), result);
        }
    }
    private Dictionary<char, Func<double[], double>>? _model = null;
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
        Dictionary<char, double> result = new();
        foreach ((char c, Func<double[], double> weight) in Model)
            result[c] = weight(Encode(biome, ancestor)) / Offset;
        return result;
    }
}