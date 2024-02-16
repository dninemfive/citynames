using d9.utl;
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
 * Regression variables:
 * - biome                  (one-hot categorical)
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
    public static IEnumerable<string> CsvLines(IEnumerable<(string cityName, string biome)> rawData, int contextLength = 2)
    {
        yield return Datum.CsvHeader;
        foreach ((string cityName, string biome) in rawData)
            foreach (Datum datum in DataFrom(cityName, biome, contextLength))
                yield return datum.CsvLine;
    }
    public static void WriteCsv(int contextLength = 2)
    {
        Console.WriteLine($"{nameof(WriteCsv)}({contextLength})");
        List<(string cityName, string biome)> allCityData = Querier.GetAllCityDataAsync()
                                                                   .ToBlockingEnumerable()
                                                                   .ToList();
        List<char> alphabet = allCityData.Select(x => x.cityName)
                                         .SelectMany(x => x.AsEnumerable())
                                         .Distinct()
                                         .Where(x => x != ',')
                                         .Order()
                                         .ToList();
        List<string> biomes = allCityData.Select(x => x.biome.Replace(',', '_'))
                                         .Distinct()
                                         .Order()
                                         .ToList();
        Dictionary<(string context, string biome), CountingDictionary<char, int>> processedData = new();
        void Add(string context, string biome, char successor)
        {
            biome = biome.Replace(',', '_');
            Console.WriteLine($"\t\tAdd({context}, {biome}, {successor} ({(int)successor}))");
            if (!processedData.TryGetValue((context, biome), out CountingDictionary<char, int>? dict))
                dict = new();
            dict.Increment(successor);
            processedData[(context, biome)] = dict;
        }
        foreach(string biome in biomes)
        {
            Console.WriteLine($"\t{biome}");
            List<string> cityNames = allCityData.Where(x => x.biome == biome)
                                                .Select(x => x.cityName)
                                                .ToList();
            foreach (string cityName in cityNames)
                foreach (Datum datum in DataFrom(cityName, biome, contextLength))
                {
                    if (datum.Successor == ',')
                        break;
                    Add(datum.Context, biome, datum.Successor);
                }
        }
        string fileName = $"transformedData.csv";
        File.WriteAllText(fileName, "");
        using FileStream fs = File.OpenWrite(fileName);
        using StreamWriter sw = new(fs);
        sw.WriteLine($"context,successor,biome,count");
        foreach ((string context, string biome) in processedData.Keys.OrderBy(x => x.context)
                                                                     .ThenBy(x => x.biome))
        {
            if (processedData.TryGetValue((context, biome), out CountingDictionary<char, int>? contextData))
            {
                foreach (char c in contextData.Keys.Order())
                {
                    Console.WriteLine($"{context},{c},{biome},{contextData[c]}");
                    sw.WriteLine($"{context},{c},{biome},{contextData[c]}");
                }
            }
        }
    }
}