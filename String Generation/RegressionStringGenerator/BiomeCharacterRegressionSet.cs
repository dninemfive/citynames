using d9.utl;
using System.Text.Json.Serialization;

namespace citynames;
public class BiomeCharacterRegressionSet
{
    [JsonInclude]
    public OneHotEncoding<string> BiomeEncoding { get; private set; }
    [JsonInclude]
    public OneHotEncoding<char> CharacterEncoding { get; private set; }
    [JsonInclude]
    public int MaxOffset { get; private set; }
    private readonly Dictionary<int, BiomeCharacterRegression> _regressions = new();
    public BiomeCharacterRegressionSet(OneHotEncoding<string> biomeEncoding, OneHotEncoding<char> characterEncoding, int maxOffset)
    {
        BiomeEncoding = biomeEncoding;
        CharacterEncoding = characterEncoding;
        MaxOffset = maxOffset;
        for (int i = 1; i < maxOffset; i++)
            _regressions[i] = new(biomeEncoding, characterEncoding, i);
    }
    public IReadOnlyDictionary<char, double> WeightsFor(string biome, char ancestor)
    {
        DefaultDict<char, double> result = new(0);
        Console.WriteLine($"Regressions: {_regressions.Select(x => $"{x.Key}: {x.Value}").ListNotation()}");
        foreach ((int offset, BiomeCharacterRegression regression) in _regressions)
        {
            foreach ((char character, double weight) in regression.WeightsFor(biome, ancestor))
                result[character] += weight;
        }
        return result;
    }
    public void AddMany(IEnumerable<(string city, string biome)> data)
    {
        IEnumerable<CharPair> pairs = CharPair.From(data.Select(x => (x.city.SandwichWith(Characters.START, Characters.STOP), x.biome)), MaxOffset);
        foreach (BiomeCharacterRegression regression in _regressions.Values)
            regression.AddMany(pairs);
    }
}