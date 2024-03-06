using d9.utl;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace citynames;
[Generator("markov", "generators_{contextLength}.json")]
public class EnhancedMarkovStringGenerator
    : IBuildLoadableStringGenerator<NgramInfo, EnhancedMarkovStringGenerator>
{
    public int ContextLength { get; private set; }
    private readonly Dictionary<string, MarkovCharacterGenerator> _dict;
    private readonly MarkovCharacterGenerator _prior;
    private readonly Func<float, float> _activationFunction;
    private float DefaultActivationFunction(float input)
        => (float)Math.Max(Math.Pow(input, 9), 0);
    public EnhancedMarkovStringGenerator(int contextLength = 2) : this(null, null, null, contextLength) { }
    public EnhancedMarkovStringGenerator(Dictionary<string, 
                                         MarkovCharacterGenerator>? dict = null, 
                                         MarkovCharacterGenerator? prior = null, 
                                         Func<float, float>? activationFunction = null, 
                                         int contextLength = 2)
    {
        ContextLength = contextLength;
        _dict = dict ?? new();
        _prior = prior ?? new(2);
        _activationFunction = activationFunction ?? DefaultActivationFunction;
    }
    internal MarkovCharacterGenerator this[string key]
    {
        get => _dict[key];
        set => _dict[key] = value;
    }
    internal bool TryGetValue(string key, [NotNullWhen(true)]out MarkovCharacterGenerator? value)
        => _dict.TryGetValue(key, out value);
    private string RandomString(Dictionary<string, float> biomeWeights, int minLength, int maxLength)
    {
        MarkovCharacterGenerator ensemble = biomeWeights.Select(x => _dict[x.Key] * x.Value)
                                                        .Sum();
    }
    private float CharacterWeight(MarkovCharacterGenerator ensemble, string context, string character)
    {
        float priorWeight = _activationFunction(_prior[context])
    }
    internal IEnumerable<string> Biomes => _dict.Keys;
    public static EnhancedMarkovStringGenerator Load(string path)
        => new(JsonSerializer.Deserialize<Dictionary<string, MarkovCharacterGenerator>>(File.ReadAllText(path))!);
    public async Task SaveAsync(string path)
        => await Task.Run(() => File.WriteAllText(path, JsonSerializer.Serialize(this)));
    public static EnhancedMarkovStringGenerator Build(IEnumerable<NgramInfo> ngrams, int contextLength = 2)
    {
        EnhancedMarkovStringGenerator result = new(contextLength);
        foreach (NgramInfo ngram in ngrams)
        {
            result._prior.Add(ngram);
            if (!result.TryGetValue(ngram.Biome, out MarkovCharacterGenerator? generator))
            {
                generator = new(contextLength);
                result[ngram.Biome] = generator;
            }
            generator.Add(ngram);
        }
        return result;
    }
}