using System.Diagnostics.CodeAnalysis;

namespace citynames;
internal class GeneratorSet
{
    private readonly Dictionary<string, MarkovStringGenerator> _dict;
    internal GeneratorSet(Dictionary<string, MarkovStringGenerator>? dict = null)
    {
        _dict = dict ?? new();
    }
    internal MarkovStringGenerator this[string key]
    {
        get => _dict[key];
        set => _dict[key] = value;
    }
    internal bool TryGetValue(string key, [NotNullWhen(true)]out MarkovStringGenerator? value)
        => _dict.TryGetValue(key, out value);
    internal IEnumerable<string> Biomes => _dict.Keys;
}