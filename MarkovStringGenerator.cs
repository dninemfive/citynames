using d9.utl;
using System.Text.Json.Serialization;

namespace citynames;
/// <summary>
/// Models a basic <see href="https://en.wikipedia.org/wiki/Markov_chain">Markov process</see> which
/// takes a string of characters as input and produces a random character based on the input characters,
/// weighted by commonality in the source corpus. 
/// </summary>
public class MarkovStringGenerator : IStringGenerator
{
    /// <summary>
    /// The ETX (End-of-Text) character in ASCII. Used to mark the end of a word,
    /// which allows randomly-generated words to break in positions which make sense.
    /// </summary>
    public const char STOP = (char)3;
    /// <summary>
    /// Holds the corresponding weights for each character based on a given input (sub-)string.
    /// </summary>
    /// <remarks>This is <see langword="public"/> primarily to make serialization less tedious.
    /// Generally, data should only be added using <see cref="Add(string)"/>.</remarks>
    public Dictionary<string, CountingDictionary<char, int>> Data { get; private set; } = new();
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
    public MarkovStringGenerator(IEnumerable<string> data, int contextLength) : this(contextLength)
    {
        foreach (string datum in data)
            Add(datum);
    }
    [JsonConstructor]
    public MarkovStringGenerator(Dictionary<string, CountingDictionary<char, int>> data, int contextLength = 1) : this(contextLength)
    {
        Data = data;
    }
    /// <summary>
    /// Adds a new string to the generator's <see cref="Data"/>. Breaks the string down into substrings of
    /// length <see cref="ContextLength"/> and notes which character succeeds a given substring.
    /// </summary>
    /// <param name="s">The string to add to the dataset.</param>
    public void Add(string s)
    {
        if (s[^1] != STOP)
            s += STOP;
        string cur = "";
        for(int i = 1 - ContextLength; i <= s.Length - ContextLength; i++)
        {
            if (!Data.ContainsKey(cur))
                Data[cur] = new();
            Data[cur].Increment(s[i + ContextLength - 1]);
            cur = s.SubstringSafe(i, i + ContextLength);
        }        
    }
    [JsonIgnore]
    public string RandomString
    {
        get
        {
            if (!Data.Any())
                throw new InvalidOperationException("Attempted to generate from a markov string generator with no data!");
            string context = "", result = "";
            while(true)
            {
                if(Data.TryGetValue(context, out CountingDictionary<char, int>? dict))
                {
                    context = $"{context}{dict.WeightedRandomElement(x => x.Value).Key}".Last(ContextLength);
                    if (context.Contains(STOP))
                        break;
                }
                else
                {
                    break;
                }
                result += context.Last();
            }
            return result.Replace($"{STOP}","");
        }
    }
    public string RandomStringOfLength(int min = 1, int max = int.MaxValue, int maxAttempts = 100)
    {
        string result = "";
        int ct = 0;
        while (result.Length < min || result.Length > max)
        {
            result = RandomString;
            if (++ct == maxAttempts)
            {
                Console.WriteLine($"Failed to generate random string with target length [{min}..{max}] after {maxAttempts} attempts.");
                break;
            }
        }
        return result;
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
                foreach (char cc in Data[s].Keys.Order())
                    lines.Add($"\t{cc} ({(int)cc}): {Data[s][cc]}");
            }
            return lines.Aggregate((x, y) => $"{x}\n{y}");
        }
    }
    public string MostCommonPairs(int itemCountLimit = int.MaxValue)
    {
        List<(string pair, int count)> pairs = new();
        foreach(string s in Data.Keys)
        {
            foreach(char cc in Data[s].Keys)
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
