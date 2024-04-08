using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace citynames;
[Generator("markov", "markov_{contextLength}.json")]
public class MarkovSetStringGenerator : IBuildLoadableStringGenerator<CityInfo, MarkovSetStringGenerator>
{
    private readonly Dictionary<string, MarkovStringGenerator> _dict;
    internal MarkovSetStringGenerator(Dictionary<string, MarkovStringGenerator>? dict = null)
        => _dict = dict ?? new();
    internal MarkovStringGenerator this[string key]
    {
        get => _dict[key];
        set => _dict[key] = value;
    }
    internal bool TryGetValue(string key, [NotNullWhen(true)] out MarkovStringGenerator? value)
        => _dict.TryGetValue(key, out value);
    public string RandomString(CityInfo query, int _, int maxLength)
        => this[query.Biome].RandomString(NgramInfo.Query(query.Biome), maxLength);
    internal IEnumerable<string> Biomes => _dict.Keys;
    public static MarkovSetStringGenerator Load(string path)
        => new(JsonSerializer.Deserialize<Dictionary<string, MarkovStringGenerator>>(File.ReadAllText(path))!);
    public async Task SaveAsync(string path)
        => await Task.Run(() => File.WriteAllText(path, JsonSerializer.Serialize(_dict)));
    public static MarkovSetStringGenerator Build(IEnumerable<(string item, CityInfo metadata)> corpus, int contextLength = 2)
        => BuildInternal(corpus.ToNgrams(contextLength), contextLength);
    private static MarkovSetStringGenerator BuildInternal(IEnumerable<NgramInfo> ngrams, int contextLength = 2)
    {
        MarkovSetStringGenerator result = new();
        foreach (NgramInfo ngram in ngrams)
        {
            if (!result.TryGetValue(ngram.Biome, out MarkovStringGenerator? generator))
            {
                generator = new(contextLength);
                result[ngram.Biome] = generator;
            }
            generator.Add(ngram);
        }
        return result;
    }
    [JsonIgnore]
    public HashSet<string> Alphabet => _dict.SelectMany(x => x.Value.Data.Values.SelectMany(y => y.Keys)).Distinct().ToHashSet();
}