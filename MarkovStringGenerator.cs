using d9.utl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace citynames;
public class MarkovStringGenerator
{
    // STX and ETX in ASCII
    public const char START = (char)2, STOP = (char)3;
    readonly Dictionary<char, CountingDictionary<char, int>> _data = new();
    public MarkovStringGenerator() { }
    public MarkovStringGenerator(IEnumerable<string> data)
    {
        foreach (string datum in data)
            Add(datum);
    }
    public void Add(string s)
    {
        // Console.WriteLine($"Add({s})");
        if (s[^1] != STOP)
            s += STOP;
        char cur = START;
        for(int i = 0; i < s.Length; i++)
        {
            if (!_data.ContainsKey(cur))
                _data[cur] = new();
            // Console.WriteLine($"{"".PadLeft(i)}{cur}");
            _data[cur].Increment(s[i]);
            cur = s[i];
        }        
    }
    public string RandomString
    {
        get
        {
            if (!_data.Any())
                throw new InvalidOperationException("Attempted to generate from a markov string generator with no data!");
            char cur = START;
            string result = "";
            for(int i = 0; cur != STOP; i++)
            {
                if(_data.TryGetValue(cur, out CountingDictionary<char, int>? dict))
                {
                    cur = dict.WeightedRandomElement(x => x.Value).Key;
                }
                else
                {
                    break;
                }
                result += cur;
            }
            if (result.Length < 1)
                return "";
            return result[..^1];
        }
    }
    public string DataString
    {
        get
        {
            List<string> lines = new();
            foreach(char c in _data.Keys.Order())
            {
                lines.Add($"{c} ({(int)c}):");
                foreach (char cc in _data[c].Keys.Order())
                    lines.Add($"\t{cc} ({(int)cc}): {_data[c][cc]}");
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
        foreach(char c in _data.Keys)
        {
            foreach(char cc in _data[c].Keys)
            {
                pairs.Add(($"{c}{cc}", _data[c][cc]));
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
