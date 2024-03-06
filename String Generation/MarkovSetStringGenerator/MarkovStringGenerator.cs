using d9.utl;
using System.Text.Json.Serialization;

namespace citynames;
/// <summary>
/// Models a basic <see href="https://en.wikipedia.org/wiki/Markov_chain">Markov process</see> which
/// takes a string of characters as input and produces a random character based on the input characters,
/// weighted by commonality in the source corpus. 
/// </summary>
public class MarkovStringGenerator
{
    /// <summary>
    /// Holds the corresponding weights for each character based on a given input (sub-)string.
    /// </summary>
    /// <remarks>This is <see langword="public"/> primarily to make serialization less tedious.
    /// Generally, data should only be added using <see cref="Add(string)"/>.</remarks>
    public Dictionary<string, CountingDictionary<string, int>> Data { get; private set; } = new();
    /// <summary>
    /// The length of input string used to determine the next character. Note that this is only
    /// an upper bound, as substrings at the beginnings of words will be shorter.<br/><br/>
    /// 
    /// This can only be set when creating a new instance because the generated data will necessarily
    /// be different for different contexts.
    /// </summary>
    /// <remarks>In testing, a context length of 2 appeared to be the sweet spot between barely
    /// recognizable gibberish and just generating real-life cities.</remarks>
    public int ContextLength { get; private set; }
    public MarkovStringGenerator(int contextLength) 
    {
        ContextLength = contextLength;
    }
    [JsonConstructor]
    public MarkovStringGenerator(Dictionary<string, CountingDictionary<string, int>> data, int contextLength = 1) : this(contextLength)
    {
        Data = data;
    }
    public void Add(NgramInfo ngram)
    {
        (string context, string result, string _) = ngram;
        if (!Data.ContainsKey(context))
            Data[context] = new();
        Data[context].Increment(result);
    }
    public void Add(IEnumerable<NgramInfo> ngrams)
    {
        foreach (NgramInfo ngram in ngrams)
            Add(ngram);
    }
    public string RandomString(NgramInfo query, int maxLength = 100)
    {
        if (!Data.Any())
            throw new InvalidOperationException("Attempted to generate from a markov string generator with no data!");
        string context = query.Context.Last(ContextLength), result = query.Context;
        while (result.Length < maxLength)
        {
            if (Data.TryGetValue(context, out CountingDictionary<string, int>? dict))
            {
                context = $"{context}{dict.WeightedRandomElement(x => x.Value).Key}"
                                          .Last(ContextLength);
                if (context.Contains(Characters.STOP))
                    break;
            }
            else
            {
                break;
            }
            result += context.Last();
        }
        return result.Replace($"{Characters.STOP}", "");
    }
    [JsonIgnore]
    public string DataString
    {
        get
        {
            List<string> lines = new();
            foreach (string s in Data.Keys.Order())
            {
                lines.Add($"{s} ({(int)s[0]}):");
                foreach (string cc in Data[s].Keys.Order())
                    lines.Add($"\t{cc} ({(int)cc[0]}): {Data[s][cc]}");
            }
            return lines.Aggregate((x, y) => $"{x}\n{y}");
        }
    }
    public string MostCommonPairs(int itemCountLimit = int.MaxValue)
    {
        List<(string pair, int count)> pairs = new();
        foreach(string s in Data.Keys)
        {
            foreach(string cc in Data[s].Keys)
            {
                pairs.Add(($"{s}{cc}", Data[s][cc]));
            }
        }
        List<string> lines = new();
        int rank = 0;
        lines.Add($"rank  count  pair  codes");
        lines.Add($"----  -----  ----  -----");
        foreach((string pair, int count) in pairs.OrderByDescending(x => x.count))
        {
            if (++rank > itemCountLimit)
                break;
            lines.Add($"{rank,4}. {count,5}  {pair,-6}  {(int)pair[0],4};{(int)pair[1],4}");
        }
        return lines.Aggregate((x, y) => $"{x}\n{y}");
    }
}
