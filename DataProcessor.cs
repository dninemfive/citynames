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
    public static IEnumerable<NgramInfo> DataFrom(string cityName, string biome, int contextLength = 2)
    {
        cityName = cityName.AppendIfNotPresent(STOP);
        string cur = "";
        for (int i = 1 - contextLength; i <= cityName.Length - contextLength; i++)
        {
            yield return new(cur, cityName[i + contextLength - 1], biome);
            cur = cityName.SubstringSafe(i, i + contextLength);
        }
    }
    public static IEnumerable<NgramInfo> Process(IEnumerable<(string cityName, string biome)> rawData, int contextLength = 2)
    {
        foreach ((string cityName, string biome) in rawData)
        {
            foreach (NgramInfo datum in DataFrom(cityName, biome, contextLength))
            {
                if (datum.Successor == ',')
                    break;
                yield return datum;
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
        foreach(NgramInfo datum in Process(allCityData, contextLength))
            write(datum.CsvLine);
    }
}