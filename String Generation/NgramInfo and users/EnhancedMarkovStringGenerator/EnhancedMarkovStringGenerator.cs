using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace citynames;
[Generator("enhanced-markov", "enhanced_markov_{contextLength}.json")]
public class EnhancedMarkovStringGenerator
    : IBuildLoadableStringGenerator<CityInfo, EnhancedMarkovStringGenerator>
{
    public int ContextLength { get; private set; }
    private readonly Dictionary<string, MarkovCharacterGenerator> _dict;
    private readonly MarkovCharacterGenerator _prior;
    private readonly Func<double, double> _activationFunction;
    private double DefaultActivationFunction(double input)
        => 0.01f;
    public EnhancedMarkovStringGenerator(int contextLength = 2) : this(null, null, null, contextLength) { }
    public EnhancedMarkovStringGenerator(Dictionary<string,
                                         MarkovCharacterGenerator>? dict = null,
                                         MarkovCharacterGenerator? prior = null,
                                         Func<double, double>? activationFunction = null,
                                         int contextLength = 2)
    {
        ContextLength = contextLength;
        _dict = dict ?? new();
        _prior = prior ?? new(2);
        _activationFunction = activationFunction ?? DefaultActivationFunction;
    }
    public MarkovCharacterGenerator this[string key]
    {
        get => _dict[key];
        set => _dict[key] = value;
    }
    public bool TryGetValue(string key, [NotNullWhen(true)] out MarkovCharacterGenerator? value)
        => _dict.TryGetValue(key, out value);
    private MarkovCharacterGenerator WeightedEnsemble(IReadOnlyDictionary<string, double> biomeWeights)
    {
        MarkovCharacterGenerator ensemble = biomeWeights.Select(x => _dict[x.Key] * x.Value)
                                                        .Sum();
        double weightRatio = ensemble.TotalWeight / (_prior.TotalWeight + ensemble.TotalWeight), adjustedWeight = _activationFunction(weightRatio);
        return _prior + (ensemble * (_prior.TotalWeight / ensemble.TotalWeight));
    }
    public string RandomString(IReadOnlyDictionary<string, double> biomeWeights, int _, int maxLength)
    {
        MarkovCharacterGenerator weightedEnsemble = WeightedEnsemble(biomeWeights);
        string result = "";
        while (result.Length < maxLength && weightedEnsemble.RandomCharacter(result.Last(ContextLength), out string? next))
            result += next;
        return result;
    }
    public string RandomString(CityInfo query, int _, int maxLength)
        => RandomString(query.Biome.ToWeightVector(), _, maxLength);
    public IEnumerable<string> Biomes => _dict.Keys;
    public static EnhancedMarkovStringGenerator Load(string path)
        => new(JsonSerializer.Deserialize<Dictionary<string, MarkovCharacterGenerator>>(File.ReadAllText(path))!);
    public async Task SaveAsync(string path)
        => await Task.Run(() => File.WriteAllText(path, JsonSerializer.Serialize(this)));
    public static EnhancedMarkovStringGenerator Build(IEnumerable<(string item, CityInfo metadata)> corpus, int contextLength = 2)
        => BuildInternal(corpus.ToNgrams(contextLength), contextLength);
    private static EnhancedMarkovStringGenerator BuildInternal(IEnumerable<NgramInfo> ngrams, int contextLength = 2)
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