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
    public static IEnumerable<NgramInfo> ToNgrams(this IEnumerable<(string cityName, string biome)> rawData, int contextLength = 2, string breakChars = ",(")
    {
        foreach ((string cityName, string biome) in rawData)
        {
            foreach (NgramInfo ngram in cityName.NgramInfos(biome, contextLength))
            {
                if (breakChars.Contains(ngram.Successor))
                    break;
                yield return ngram;
            }
        }
    }
    public static async IAsyncEnumerable<NgramInfo> ToNgramsAsync(this IAsyncEnumerable<(string cityName, string biome)> rawData, int contextLength = 2, string breakChars = ",(")
    {
        await foreach((string cityName, string biome) in rawData)
        {
            foreach(NgramInfo ngram in cityName.NgramInfos(biome, contextLength))
            {
                if (breakChars.Contains(ngram.Successor))
                    break;
                yield return ngram;
            } 
        } 
    } 
    public static void WriteCsv(int contextLength = 2, bool writeToConsole = false)
    {
        Console.WriteLine($"{nameof(WriteCsv)}({contextLength})");
        List<(string cityName, string biome)> allCityData = Querier.GetAllCityDataAsync()
                                                                   .ToBlockingEnumerable()
                                                                   .ToList();
        string fileName = $"transformedData.csv";
        File.WriteAllText(fileName, "");
        using FileStream fs = File.OpenWrite(fileName);
        using StreamWriter sw = new(fs);
        void write(string? s)
        {
            sw.WriteLine(s);
            if (writeToConsole)
                Console.WriteLine(s);
        }
        write(NgramInfo.CsvHeader);
        foreach(NgramInfo datum in allCityData.ToNgrams(contextLength))
            write(datum.CsvLine);
    }
}