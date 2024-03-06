using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace citynames;
[Generator("markov", "generators_{contextLength}.json")]
public class EnhancedMarkovStringGenerator
    : IBuildLoadableStringGenerator<NgramInfo, EnhancedMarkovStringGenerator>
{
    public int ContextLength { get; private set; }
    private readonly Dictionary<string, MarkovStringGenerator> _dict;
    private readonly MarkovStringGenerator _prior;
    public EnhancedMarkovStringGenerator(int contextLength = 2) : this(null, null, contextLength) { }
    public EnhancedMarkovStringGenerator(Dictionary<string, MarkovStringGenerator>? dict = null, MarkovStringGenerator? prior = null, int contextLength = 2)
    {
        ContextLength = contextLength;
        _dict = dict ?? new();
        _prior = prior ?? new(2);
    }
    internal MarkovStringGenerator this[string key]
    {
        get => _dict[key];
        set => _dict[key] = value;
    }
    internal bool TryGetValue(string key, [NotNullWhen(true)]out MarkovStringGenerator? value)
        => _dict.TryGetValue(key, out value);
    public string RandomString(NgramInfo input, int _, int maxLength)
        => this[input.Biome].RandomString(input, maxLength);
    internal IEnumerable<string> Biomes => _dict.Keys;
    public static EnhancedMarkovStringGenerator Load(string path)
        => new(JsonSerializer.Deserialize<Dictionary<string, MarkovStringGenerator>>(File.ReadAllText(path))!);
    public async Task SaveAsync(string path)
        => await Task.Run(() => File.WriteAllText(path, JsonSerializer.Serialize(this)));
    public static EnhancedMarkovStringGenerator Build(IEnumerable<NgramInfo> ngrams, int contextLength = 2)
    {
        EnhancedMarkovStringGenerator result = new(contextLength);
        foreach (NgramInfo ngram in ngrams)
        {
            result._prior.Add(ngram);
            if (!result.TryGetValue(ngram.Biome, out MarkovStringGenerator? generator))
            {
                generator = new(contextLength);
                result[ngram.Biome] = generator;
            }
            generator.Add(ngram);
        }
        return result;
    }
    public HashSet<string> Alphabet => _dict.SelectMany(x => x.Value.Data.Values.SelectMany(y => y.Keys)).Distinct().ToHashSet();
}