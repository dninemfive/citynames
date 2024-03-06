using d9.utl;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Text.Json.Serialization;

namespace citynames;
/// <summary>
/// Models a basic <see href="https://en.wikipedia.org/wiki/Markov_chain">Markov process</see> which
/// takes a string of characters as input and produces a random character based on the input characters,
/// weighted by commonality in the source corpus. 
/// </summary>
public class MarkovCharacterGenerator(int contextLength)
    : IAdditionOperators<MarkovCharacterGenerator, MarkovCharacterGenerator, MarkovCharacterGenerator>
{
    /// <summary>
    /// Holds the corresponding weights for each character based on a given input (sub-)string.
    /// </summary>
    /// <remarks>This is <see langword="public"/> primarily to make serialization less tedious.
    /// Generally, data should only be added using <see cref="Add(string)"/>.</remarks>
    public Dictionary<string, CharacterDistribution> Data { get; private set; } = new();
    /// <summary>
    /// The length of input string used to determine the next character. Note that this is only
    /// an upper bound, as substrings at the beginnings of words will be shorter.<br/><br/>
    /// 
    /// This can only be set when creating a new instance because the generated data will necessarily
    /// be different for different contexts.
    /// </summary>
    /// <remarks>In testing, a context length of 2 appeared to be the sweet spot between barely
    /// recognizable gibberish and just generating real-life cities.</remarks>
    public int ContextLength { get; private set; } = contextLength;

    [JsonConstructor]
    public MarkovCharacterGenerator(Dictionary<string, CharacterDistribution> data, int contextLength = 2) : this(contextLength)
    {
        Data = data;
    }
    public void Add(NgramInfo ngram)
    {
        (string context, string result, string _) = ngram;
        if (!Data.ContainsKey(context))
            Data[context] = new();
        Data[context].Add(result);
    }
    public void Add(IEnumerable<NgramInfo> ngrams)
    {
        foreach (NgramInfo ngram in ngrams)
            Add(ngram);
    }
    public bool RandomCharacter(string context, [NotNullWhen(true)]out string? result, bool warnOnKeyFailure = false)
    {
        result = null;
        if (!Data.Any())
            throw new InvalidOperationException($"Attempted to generate from a {nameof(MarkovCharacterGenerator)} with no data!");
        Data.TryGetValue(context, out CharacterDistribution? weights);
        if (weights is not null)
        {
            result = weights.WeightedRandomElement(x => x.Value).Key;
            return !result.Contains(Characters.STOP);
        }
        if (warnOnKeyFailure)
            Console.WriteLine($"Key lookup failure in {nameof(MarkovCharacterGenerator)}: {context} was not found!");
        return false;
    }
    public CharacterDistribution this[string context] => Data[context];
    public static MarkovCharacterGenerator operator *(MarkovCharacterGenerator mcg, float factor)
    {
        Dictionary<string, CharacterDistribution> result = new();
        foreach((string key, CharacterDistribution value) in mcg.Data)
            result[key] = value * factor;
        return new(result);
    }
    public static MarkovCharacterGenerator operator+(MarkovCharacterGenerator a, MarkovCharacterGenerator b)
    {
        Dictionary<string, CharacterDistribution> result = new();
        foreach(string key in a.Data.Keys.Union(b.Data.Keys))
        {
            a.Data.TryGetValue(key, out CharacterDistribution? aDist);
            b.Data.TryGetValue(key, out CharacterDistribution? bDist);
            result[key] = (aDist ?? new()) + (bDist ?? new());
        }
        return new(result);
    }
    public float TotalWeight
        => Data.Select(x => x.Value.Weight).Sum();
}
