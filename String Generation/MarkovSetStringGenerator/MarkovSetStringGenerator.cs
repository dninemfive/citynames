using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace citynames;
internal class MarkovSetStringGenerator : IBuildLoadAbleStringGenerator<NgramInfo, MarkovSetStringGenerator>
{
    private readonly Dictionary<string, MarkovStringGenerator> _dict;
    internal MarkovSetStringGenerator(Dictionary<string, MarkovStringGenerator>? dict = null)
        => _dict = dict ?? new();
    internal MarkovStringGenerator this[string key]
    {
        get => _dict[key];
        set => _dict[key] = value;
    }
    internal bool TryGetValue(string key, [NotNullWhen(true)]out MarkovStringGenerator? value)
        => _dict.TryGetValue(key, out value);
    public string RandomString(NgramInfo input, int _)
        => this[input.Biome].RandomString;
    internal IEnumerable<string> Biomes => _dict.Keys;
    public static async Task<MarkovSetStringGenerator> LoadAsync(string path)
        => new(await Task.Run(() => JsonSerializer.Deserialize<Dictionary<string, MarkovStringGenerator>>(File.ReadAllText(path))!));
    public async Task SaveAsync(string path)
        => await Task.Run(() => File.WriteAllText(path, JsonSerializer.Serialize(this)));
    public static async Task<MarkovSetStringGenerator> BuildAsync(IAsyncEnumerable<NgramInfo> ngrams, int contextLength = 2)
    {
        MarkovSetStringGenerator result = new();
        await foreach (NgramInfo ngram in ngrams)
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
    public HashSet<string> Alphabet => _dict.SelectMany(x => x.Value.Data.Values.SelectMany(y => y.Keys)).Distinct().ToHashSet();
}