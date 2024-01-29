using d9.utl;
using System.Text.Json.Serialization;

namespace citynames;
public class MarkovStringGenerator
{
    // STX and ETX in ASCII
    public const char START = (char)2, STOP = (char)3;
    public Dictionary<string, CountingDictionary<char, int>> Data { get; private set; } = new();
    public MarkovStringGenerator() { }
    public MarkovStringGenerator(IEnumerable<string> data)
    {
        foreach (string datum in data)
            Add(datum);
    }
    [JsonConstructor]
    public MarkovStringGenerator(Dictionary<string, CountingDictionary<char, int>> data)
    {
        Data = data;
    }
    public void Add(string s)
    {
        // Console.WriteLine($"Add({s})");
        if (s[^1] != STOP)
            s += STOP;
        string cur = "";
        for(int i = 0; i < s.Length; i++)
        {
            if (!Data.ContainsKey(cur))
                Data[cur] = new();
            // Console.WriteLine($"{"".PadLeft(i)}{cur}");
            Data[cur].Increment(s[i]);
            cur = s.SubstringSafe(i, i + 1);
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
                    context = $"{dict.WeightedRandomElement(x => x.Value).Key}";
                }
                else
                {
                    break;
                }
                result += context;
            }
            return result.Replace($"{STOP}","");
        }
    }
    [JsonIgnore]
    public string DataString
    {
        get
        {
            List<string> lines = new();
            foreach(string s in Data.Keys.Order())
            {
                lines.Add($"{s} ({(int)s[0]}):");
                foreach (char cc in Data[s].Keys.Order())
                    lines.Add($"\t{cc} ({(int)cc}): {Data[s][cc]}");
            }
            return lines.Aggregate((x, y) => $"{x}\n{y}");
        }
    }
    public string RandomStringOfLength(int min = 1, int max = int.MaxValue)
    {
        string result = "";
        while (result.Length < min || result.Length > max)
        {
            result = RandomString;
            //Console.WriteLine($"Considering: {result,-100} ({result.Length})");
        }
        return result;
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
