using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace citynames;
internal class MarkovSetStringGenerator : IStringGenerator<NgramInfo>, IAsyncSaveLoadable<MarkovSetStringGenerator>
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
    public string RandomString(NgramInfo input)
        => this[input.Biome].RandomString;
    internal IEnumerable<string> Biomes => _dict.Keys;
    public static async Task<MarkovSetStringGenerator> LoadAsync(string path)
        => new(await Task.Run(() => JsonSerializer.Deserialize<Dictionary<string, MarkovStringGenerator>>(File.ReadAllText(path))!));
    public async Task SaveAsync(string path)
        => await Task.Run(() => File.WriteAllText(path, JsonSerializer.Serialize(this)));
}