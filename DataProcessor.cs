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
    public static void WriteCsvs(int contextLength = 2)
    {
        Console.WriteLine($"{nameof(WriteCsvs)}({contextLength})");
        List<(string cityName, string biome)> allCityData = Querier.GetAllCityDataAsync()
                                                                   .ToBlockingEnumerable()
                                                                   .ToList();
        List<char> alphabet = allCityData.Select(x => x.cityName)
                                         .SelectMany(x => x.AsEnumerable())
                                         .Distinct()
                                         .Where(x => x != ',')
                                         .Order()
                                         .ToList();
        List<string> biomes = allCityData.Select(x => x.biome)
                                         .Distinct()
                                         .Order()
                                         .ToList();
        Dictionary<(string context, string biome), CountingDictionary<char, int>> processedData = new();
        void Add(string context, string biome, char successor)
        {
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
                    Add(datum.Context, biome, datum.Successor);
        }
        _ = Directory.CreateDirectory("csvs");
        foreach((string context, string _) in processedData.Keys.OrderBy(x => x.context))
        {
            string fileName = $"csvs/{context.FileNameSafe()}.csv";
            fileName.CreateIfNotExists("biome,character,count");
            foreach(string biome in biomes)
            {
                Console.WriteLine($"\t{context}: {biome}");
                CountingDictionary<char, int> contextData = processedData.TryGetValue((context, biome), out CountingDictionary<char, int>? val) ? val : new();
                foreach (char c in alphabet)
                    File.AppendAllText(fileName, $"\n{biome},{c},{contextData[c]}");
            }
        }
    }
}