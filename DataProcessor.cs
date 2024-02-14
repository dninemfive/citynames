using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace citynames;

/* 
 * Input:
 * - query                  (character)
 * - context                (string or pair of characters)
 * - biome                  (categorical)
 * - distance from coast    (float/stretch goal)
 * Output:
 * - probability that the queried character will occur
 */
public static class DataProcessor
{

    /// <summary>
    /// The ETX (End-of-Text) character in ASCII. Used to mark the end of a word,
    /// which allows randomly-generated words to break in positions which make sense.
    /// </summary>
    public const char STOP = (char)3;
    public static IEnumerable<Datum> DataFrom(string cityName, string biome, int contextLength = 2)
    {
        cityName = cityName.AppendIfNotPresent(STOP);
        string cur = "";
        for (int i = 1 - contextLength; i <= cityName.Length - contextLength; i++)
        {
            yield return new(biome, cur, cityName[i + contextLength - 1]);
            cur = cityName.SubstringSafe(i, i + contextLength);
        }
    }
}
public class Datum
{
    public string Biome;
    public string Context;
    public char Result;
    public Datum(string biome, string context, char result)
    {
        Biome = biome;
        Context = context;
        Result = result;
    }
    public void Deconstruct(out string biome, out string context, out char result)
    {
        biome = Biome;
        context = Context;
        result = Result;
    }
    public string CsvString => $"{Biome},{Context},{Result}";
    public override string ToString()
        => $"{Biome}\t{Context}\t{Result}\t({(int)Result})";
    public override bool Equals(object? obj)
        => obj is Datum d && d.Biome == Biome && d.Context == Context && d.Result == Result;
    public override int GetHashCode()
        => HashCode.Combine(Biome, Context, Result);
    public static bool operator ==(Datum a, Datum b) => a.Equals(b);
    public static bool operator !=(Datum a, Datum b) => !(a == b);
}