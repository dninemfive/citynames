using System.Diagnostics.CodeAnalysis;

namespace citynames;
/// <summary>
/// Just an alias for the <see cref="Dictionary{TKey, TValue}">Dictionary</see>&lt;<see langword="string"/>, <see cref="MarkovStringGenerator"/>&gt;
/// type since that was creating unnecessarily verbose code.
/// </summary>
internal class GeneratorSet
{
    private readonly Dictionary<string, MarkovStringGenerator> _dict;
    internal GeneratorSet(Dictionary<string, MarkovStringGenerator>? dict = null)
        => _dict = dict ?? new();
    internal MarkovStringGenerator this[string key]
    {
        get => _dict[key];
        set => _dict[key] = value;
    }
    internal bool TryGetValue(string key, [NotNullWhen(true)]out MarkovStringGenerator? value)
        => _dict.TryGetValue(key, out value);
    internal IEnumerable<string> Biomes => _dict.Keys;
}